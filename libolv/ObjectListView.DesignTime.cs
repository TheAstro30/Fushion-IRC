/* Object List View
 * Copyright (C) 2006-2012 Phillip Piper
 * Refactored by Jason James Newland - 2014
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * If you wish to use this code in a closed source application, please contact phillip_piper@bigfoot.com.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using libolv.Rendering.Overlays;

namespace libolv
{
    public class ObjectListViewDesigner : ControlDesigner
    {
        protected ControlDesigner ListViewDesigner;
        protected IDesignerFilter DesignerFilter;
        protected MethodInfo ListViewDesignGetHitTest;
        protected MethodInfo ListViewDesignWndProc;

        public override void Initialize(IComponent component)
        {            
            /* Use reflection to bypass the "internal" marker on ListViewDesigner
             * If we can't get the unversioned designer, look specifically for .NET 4.0 version of it. */
            var tListViewDesigner = Type.GetType("System.Windows.Forms.Design.ListViewDesigner, System.Design") ??
                                    Type.GetType("System.Windows.Forms.Design.ListViewDesigner, System.Design, " +
                                                 "Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            if (tListViewDesigner == null) { throw new ArgumentException("Could not load ListViewDesigner"); }
            ListViewDesigner = (ControlDesigner)Activator.CreateInstance(tListViewDesigner, BindingFlags.Instance | BindingFlags.Public, null, null, null);
            DesignerFilter = ListViewDesigner;
            /* Fetch the methods from the ListViewDesigner that we know we want to use */
            ListViewDesignGetHitTest = tListViewDesigner.GetMethod("GetHitTest", BindingFlags.Instance | BindingFlags.NonPublic);
            ListViewDesignWndProc = tListViewDesigner.GetMethod("WndProc", BindingFlags.Instance | BindingFlags.NonPublic);

            Debug.Assert(ListViewDesignGetHitTest != null, "Required method (GetHitTest) not found on ListViewDesigner");
            Debug.Assert(ListViewDesignWndProc != null, "Required method (WndProc) not found on ListViewDesigner");
            /* Tell the Designer to use properties of default designer as well as the properties of this class (do before base.Initialize) */
            TypeDescriptor.CreateAssociation(component, ListViewDesigner);
// ReSharper disable SuspiciousTypeConversion.Global
            var site = (IServiceContainer)component.Site;
// ReSharper restore SuspiciousTypeConversion.Global
            if (site != null && GetService(typeof(DesignerCommandSet)) == null)
            {
                site.AddService(typeof(DesignerCommandSet), new CdDesignerCommandSet(this));
            }
            else
            {
                Debug.Fail("site != null && GetService(typeof (DesignerCommandSet)) == null");
            }
            ListViewDesigner.Initialize(component);
            base.Initialize(component);
            RemoveDuplicateDockingActionList();
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
            ListViewDesigner.InitializeNewComponent(defaultValues);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ListViewDesigner != null)
                {
                    ListViewDesigner.Dispose();
                    /* Normally we would now null out the designer, but this designer
                     * still has methods called AFTER it is disposed. */
                }
            }
            base.Dispose(disposing);
        }

        private void RemoveDuplicateDockingActionList()
        {
            /* This is a true hack -- in a class that is basically a huge hack itself.
             * Reach into the bowel of our base class, get a private field, and use that fields value to
             * remove an action from the designer.
             * In ControlDesigner, there is "private DockingActionList dockingAction;"
             * Don't you just love Reflector?! */
            var fi = typeof(ControlDesigner).GetField("dockingAction", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null)
            {
                var dockingAction = (DesignerActionList)fi.GetValue(this);
                if (dockingAction != null)
                {
                    var service = (DesignerActionService)GetService(typeof(DesignerActionService));
                    if (service != null)
                    {
                        service.Remove(Control, dockingAction);
                    }
                }
            }
        }

        /* IDesignerFilter overrides */
        protected override void PreFilterProperties(IDictionary properties)
        {
            /* Always call the base PreFilterProperties implementation 
             * before you modify the properties collection. */
            base.PreFilterProperties(properties);
            /* Give the listviewdesigner a chance to filter the properties
             * (though we already know it's not going to do anything) */
            DesignerFilter.PreFilterProperties(properties);
            /* I'd like to just remove the redundant properties, but that would
             * break backward compatibility. The deserialiser that handles the XXX.Designer.cs file
             * works off the designer, so even if the property exists in the class, the deserialiser will
             * throw an error if the associated designer actually removes that property.
             * So we shadow the unwanted properties, and give the replacement properties
             * non-browsable attributes so that they are hidden from the user */
            var unwantedProperties = new List<string>(new[]
                                                          {
                                                              "BackgroundImage", "BackgroundImageTiled", "HotTracking",
                                                              "HoverSelection",
                                                              "LabelEdit", "VirtualListSize", "VirtualMode"
                                                          });
            /* Also hide Tooltip properties, since giving a tooltip to the control through the IDE
             * messes up the tooltip handling */
            unwantedProperties.AddRange(properties.Keys.Cast<string>().Where(propertyName => propertyName.StartsWith("ToolTip")));
            /* If we are looking at a TreeListView, remove group related properties
             * since TreeListViews can't show groups */
            if (Control is TreeListView)
            {
                unwantedProperties.AddRange(new[]
                                                {
                                                    "GroupImageList", "GroupWithItemCountFormat",
                                                    "GroupWithItemCountSingularFormat", "HasCollapsibleGroups",
                                                    "SpaceBetweenGroups", "ShowGroups", "SortGroupItemsByPrimaryColumn",
                                                    "ShowItemCountOnGroups"
                                                });
            }
            /* Shadow the unwanted properties, and give the replacement properties
             * non-browsable attributes so that they are hidden from the user */
            foreach (var unwantedProperty in unwantedProperties)
            {
                var propertyDesc = TypeDescriptor.CreateProperty(
                    typeof (ObjectListView),
                    (PropertyDescriptor)properties[unwantedProperty],
                    new BrowsableAttribute(false));
                properties[unwantedProperty] = propertyDesc;
            }
        }

        protected override void PreFilterEvents(IDictionary events)
        {
            base.PreFilterEvents(events);
            DesignerFilter.PreFilterEvents(events);
            /* Remove the events that don't make sense for an ObjectListView.
             * See PreFilterProperties() for why we do this dance rather than just remove the event. */
            var unwanted = new List<string>(new[]
                                                {
                                                    "AfterLabelEdit",
                                                    "BeforeLabelEdit",
                                                    "DrawColumnHeader",
                                                    "DrawItem",
                                                    "DrawSubItem",
                                                    "RetrieveVirtualItem",
                                                    "SearchForVirtualItem",
                                                    "VirtualItemsSelectionRangeChanged"
                                                });
            /* If we are looking at a TreeListView, remove group related events
             * since TreeListViews can't show groups */
            if (Control is TreeListView)
            {
                unwanted.AddRange(new[]
                                      {
                                          "AboutToCreateGroups",
                                          "AfterCreatingGroups",
                                          "BeforeCreatingGroups",
                                          "GroupTaskClicked",
                                          "GroupExpandingCollapsing",
                                          "GroupStateChanged"
                                      });
            }
            foreach (var unwantedEvent in unwanted)
            {
                var eventDesc = TypeDescriptor.CreateEvent(
                    typeof (ObjectListView),
                    (EventDescriptor)events[unwantedEvent],
                    new BrowsableAttribute(false));
                events[unwantedEvent] = eventDesc;
            }
        }

        protected override void PostFilterAttributes(IDictionary attributes)
        {
            DesignerFilter.PostFilterAttributes(attributes);
            base.PostFilterAttributes(attributes);
        }

        protected override void PostFilterEvents(IDictionary events)
        {
            DesignerFilter.PostFilterEvents(events);
            base.PostFilterEvents(events);
        }

        /* Overrides */
        public override DesignerActionListCollection ActionLists
        {
            get
            {
                /* We want to change the first action list so it only has the commands we want */
                var actionLists = ListViewDesigner.ActionLists;
                if (actionLists.Count > 0 && !(actionLists[0] is ListViewActionListAdapter))
                {
                    actionLists[0] = new ListViewActionListAdapter(actionLists[0]);
                }
                return actionLists;
            }
        }

        public override ICollection AssociatedComponents
        {
            get
            {
                var components = new ArrayList(base.AssociatedComponents);
                components.AddRange(ListViewDesigner.AssociatedComponents);
                return components;
            }
        }

        protected override bool GetHitTest(Point point)
        {
            /* The ListViewDesigner wants to allow column dividers to be resized */
            return (bool)ListViewDesignGetHitTest.Invoke(ListViewDesigner, new object[] { point });
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x4e:
                case 0x204e:
                    /* The listview designer is interested in HDN_ENDTRACK notifications */
                    ListViewDesignWndProc.Invoke(ListViewDesigner, new object[] { m });
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /* Custom action list */
        private class ListViewActionListAdapter : DesignerActionList
        {
            private readonly DesignerActionList _wrappedList;

            public ListViewActionListAdapter(DesignerActionList wrappedList) : base(wrappedList.Component)
            {
                _wrappedList = wrappedList;
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                var items = _wrappedList.GetSortedActionItems();
                items.RemoveAt(2); /* remove Edit Groups */
                items.RemoveAt(0); /* remove Edit Items */
                return items;
            }           
        }

        /* DesignerCommandSet */
        private class CdDesignerCommandSet : DesignerCommandSet
        {
            private readonly ComponentDesigner _componentDesigner;

            public CdDesignerCommandSet(ComponentDesigner componentDesigner)
            {
                _componentDesigner = componentDesigner;
            }

            public override ICollection GetCommands(string name)
            {
                if (_componentDesigner != null)
                {
                    if (name.Equals("Verbs"))
                    {
                        return _componentDesigner.Verbs;
                    }
                    if (name.Equals("ActionLists"))
                    {
                        return _componentDesigner.ActionLists;
                    }
                }
                return base.GetCommands(name);
            }            
        }
    }

    public class OlvColumnCollectionEditor : CollectionEditor
    {
        public OlvColumnCollectionEditor(Type t) : base(t)
        {
            /* Empty */
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(OlvColumn);
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            /* Figure out which ObjectListView we are working on. This should be the Instance of the context. */
            var olv = context.Instance as ObjectListView;
            Debug.Assert(olv != null, "Instance must be an ObjectListView");
            /* Edit all the columns, not just the ones that are visible */
            base.EditValue(context, provider, olv.AllColumns);
            /* Set the columns on the ListView to just the visible columns */
            var newColumns = olv.GetFilteredColumns(View.Details);
            olv.Columns.Clear();
            foreach (var col in newColumns)
            {
                olv.Columns.Add(col);
            }
            return olv.Columns;
        }

        protected override string GetDisplayText(object value)
        {
            var col = value as OlvColumn;
            if (col == null || String.IsNullOrEmpty(col.AspectName))
            {
                return base.GetDisplayText(value);
            }
            return String.Format("{0} ({1})", base.GetDisplayText(value), col.AspectName);
        }
    }

    internal class OverlayConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var imageOverlay = value as ImageOverlay;
                if (imageOverlay != null)
                {
                    return imageOverlay.Image == null ? "(none)" : "(set)";
                }
                var textOverlay = value as TextOverlay;
                if (textOverlay != null)
                {
                    return String.IsNullOrEmpty(textOverlay.Text) ? "(none)" : "(set)";
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
