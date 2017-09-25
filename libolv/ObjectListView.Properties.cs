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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using libolv.CellEditing;
using libolv.DragDrop.DragSource;
using libolv.DragDrop.DropSink;
using libolv.Filtering;
using libolv.Filtering.Filters;
using libolv.Implementation;
using libolv.Implementation.Munger;
using libolv.Rendering.Decoration;
using libolv.Rendering.Overlays;
using libolv.Rendering.Renderers;
using libolv.Rendering.Styles;
using libolv.SubControls;

namespace libolv
{
    public partial class ObjectListView
    {
        private IModelFilter _additionalFilter;
        private List<OlvColumn> _allColumns = new List<OlvColumn>();
        private Color _alternateRowBackColor = Color.Empty;
        private SortOrder _alwaysGroupBySortOrder = SortOrder.None;
        private CellEditActivateMode _cellEditActivation = CellEditActivateMode.None;
        private CellEditKeyEngine _cellEditKeyEngine;
        private bool _cellEditTabChangesRows;
        private bool _cellEditEnterChangesRows;
        private ToolTipControl _cellToolTip;
        private StringAlignment _cellVerticalAlignment = StringAlignment.Center;
        private bool _copySelectionOnControlC = true;
        private bool _copySelectionOnControlCUsesDragSource = true;
        private readonly List<IDecoration> _decorations = new List<IDecoration>();
        private IRenderer _defaultRenderer = new BaseRenderer();
        private IDropSink _dropSink;
        private IOverlay _emptyListMsgOverlay;
        private FilterMenuBuilder _filterMenuBuilder = new FilterMenuBuilder();
        private ImageList _groupImageList;
        private HeaderControl _headerControl;
        private int _headerMaximumHeight = -1;
        private bool _headerUsesThemes = true;
        private bool _headerWordWrap;
        private HotItemStyle _hotItemStyle;
        private Color _highlightBackgroundColor = Color.Empty;
        private Color _highlightForegroundColor = Color.Empty;
        private bool _isSearchOnSortColumn = true;
        private IListFilter _listFilter;
        private IModelFilter _modelFilter;
        private IEnumerable _objects;
        private ImageOverlay _imageOverlay;
        private TextOverlay _textOverlay;
        private int _overlayTransparency = 128;
        private readonly List<IOverlay> _overlays = new List<IOverlay>();
        private bool _persistentCheckBoxes = true;
        private Dictionary<Object, CheckState> _checkStateMap;
        private OlvColumn _primarySortColumn;
        private int _rowHeight = -1;
        private SortOrder _secondarySortOrder = SortOrder.None;
        private bool _selectAllOnControlA = true;
        private ColumnSelectBehaviour _selectColumnsOnRightClickBehaviour = ColumnSelectBehaviour.InlineMenu;
        private bool _selectColumnsMenuStaysOpen = true;
        private OlvColumn _selectedColumn;
        private readonly TintedColumnDecoration _selectedColumnDecoration = new TintedColumnDecoration();
        private Color _selectedColumnTint = Color.Empty;
        private bool _showFilterMenuOnRightClick = true;
        private bool _showImagesOnSubItems;
        private bool _showHeaderInAllViews = true;
        private ImageList _shadowedImageList;
        private bool _sortGroupItemsByPrimaryColumn = true;
        private int _spaceBetweenGroups;
        private bool _tintSortColumn;
        private bool _triStateCheckBoxes;
        private bool _updateSpaceFillingColumnsWhenDraggingColumnDivider = true;
        private Color _unfocusedHighlightBackgroundColor = Color.Empty;
        private Color _unfocusedHighlightForegroundColor = Color.Empty;
        private bool _useCustomSelectionColors;
        private bool _useExplorerTheme;
        private bool _useFiltering;
        private bool _useFilterIndicator;
        private bool _useHotItem;
        private bool _useHyperlinks;
        private bool _useOverlays = true;
        private bool _useSubItemCheckBoxes;
        private bool _useTranslucentSelection;
        private bool _useTranslucentHotItem;

        private string _checkedAspectName;
        private Munger _checkedAspectMunger;
        private bool _canUseApplicationIdle = true;

        public static EditorRegistry EditorRegistry = new EditorRegistry();

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IModelFilter AdditionalFilter
        {
            get { return _additionalFilter; }
            set
            {
                if (_additionalFilter == value)
                    return;
                _additionalFilter = value;
                UpdateColumnFiltering();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public virtual List<OlvColumn> AllColumns
        {
            get { return _allColumns; }
            set { _allColumns = value ?? new List<OlvColumn>(); }
        }

        [Category("ObjectListView"),
         Description("If using alternate colors, what color should the background of alterate rows be?"),
         DefaultValue(typeof (Color), "")]
        public Color AlternateRowBackColor
        {
            get { return _alternateRowBackColor; }
            set { _alternateRowBackColor = value; }
        }

        [Browsable(false)]
        public virtual Color AlternateRowBackColorOrDefault
        {
            get { return _alternateRowBackColor == Color.Empty ? Color.LemonChiffon : _alternateRowBackColor; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual OlvColumn AlwaysGroupByColumn { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual SortOrder AlwaysGroupBySortOrder
        {
            get { return _alwaysGroupBySortOrder; }
            set { _alwaysGroupBySortOrder = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ImageList BaseSmallImageList
        {
            get { return base.SmallImageList; }
            set { base.SmallImageList = value; }
        }

        [Category("ObjectListView"), Description("How does the user indicate that they want to edit a cell?"),
         DefaultValue(CellEditActivateMode.None)]
        public virtual CellEditActivateMode CellEditActivation
        {
            get { return _cellEditActivation; }
            set
            {
                _cellEditActivation = value;
                if (Created)
                {
                    Invalidate();
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CellEditKeyEngine CellEditKeyEngine
        {
            get { return _cellEditKeyEngine ?? (_cellEditKeyEngine = new CellEditKeyEngine()); }
            set { _cellEditKeyEngine = value; }
        }

        [Browsable(false)]
        public Control CellEditor { get; private set; }

        [Category("ObjectListView"), Description("Should Tab/Shift-Tab change rows while cell editing?"),
         DefaultValue(false)]
        public virtual bool CellEditTabChangesRows
        {
            get { return _cellEditTabChangesRows; }
            set
            {
                _cellEditTabChangesRows = value;
                if (_cellEditTabChangesRows)
                {
                    CellEditKeyEngine.SetKeyBehaviour(Keys.Tab, CellEditCharacterBehaviour.ChangeColumnRight,
                                                      CellEditAtEdgeBehaviour.ChangeRow);
                    CellEditKeyEngine.SetKeyBehaviour(Keys.Tab | Keys.Shift,
                                                      CellEditCharacterBehaviour.ChangeColumnLeft,
                                                      CellEditAtEdgeBehaviour.ChangeRow);
                }
                else
                {
                    CellEditKeyEngine.SetKeyBehaviour(Keys.Tab, CellEditCharacterBehaviour.ChangeColumnRight,
                                                      CellEditAtEdgeBehaviour.Wrap);
                    CellEditKeyEngine.SetKeyBehaviour(Keys.Tab | Keys.Shift,
                                                      CellEditCharacterBehaviour.ChangeColumnLeft,
                                                      CellEditAtEdgeBehaviour.Wrap);
                }
            }
        }

        [Category("ObjectListView"), Description("Should Enter change rows while cell editing?"), DefaultValue(false)]
        public virtual bool CellEditEnterChangesRows
        {
            get { return _cellEditEnterChangesRows; }
            set
            {
                _cellEditEnterChangesRows = value;
                if (_cellEditEnterChangesRows)
                {
                    CellEditKeyEngine.SetKeyBehaviour(Keys.Enter, CellEditCharacterBehaviour.ChangeRowDown,
                                                      CellEditAtEdgeBehaviour.ChangeColumn);
                    CellEditKeyEngine.SetKeyBehaviour(Keys.Enter | Keys.Shift,
                                                      CellEditCharacterBehaviour.ChangeRowUp,
                                                      CellEditAtEdgeBehaviour.ChangeColumn);
                }
                else
                {
                    CellEditKeyEngine.SetKeyBehaviour(Keys.Enter, CellEditCharacterBehaviour.EndEdit,
                                                      CellEditAtEdgeBehaviour.EndEdit);
                    CellEditKeyEngine.SetKeyBehaviour(Keys.Enter | Keys.Shift, CellEditCharacterBehaviour.EndEdit,
                                                      CellEditAtEdgeBehaviour.EndEdit);
                }
            }
        }

        [Browsable(false)]
        public ToolTipControl CellToolTip
        {
            get
            {
                if (_cellToolTip == null)
                {
                    CreateCellToolTip();
                }
                return _cellToolTip;
            }
        }

        [Category("ObjectListView"), Description("How much padding will be applied to each cell in this control?"),
         DefaultValue(null)]
        public Rectangle? CellPadding { get; set; }

        [Category("ObjectListView"), Description("How will cell values be vertically aligned?"),
         DefaultValue(StringAlignment.Center)]
        public virtual StringAlignment CellVerticalAlignment
        {
            get { return _cellVerticalAlignment; }
            set { _cellVerticalAlignment = value; }
        }

        public new bool CheckBoxes
        {
            get { return base.CheckBoxes; }
            set
            {
                base.CheckBoxes = value;
                /* Initialize the state image list so we can display indetermined values. */
                InitializeStateImageList();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Object CheckedObject
        {
            get
            {
                var checkedObjects = CheckedObjects;
                return checkedObjects.Count == 1 ? checkedObjects[0] : null;
            }
            set { CheckedObjects = new ArrayList(new[] {value}); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IList CheckedObjects
        {
            get
            {
                var list = new ArrayList();
                if (CheckBoxes)
                {
                    for (var i = 0; i < GetItemCount(); i++)
                    {
                        var olvi = GetItem(i);
                        if (olvi.CheckState == CheckState.Checked)
                        {
                            list.Add(olvi.RowObject);
                        }
                    }
                }
                return list;
            }
            set
            {
                if (!CheckBoxes)
                {
                    return;
                }
                /* Set up an efficient way of testing for the presence of a particular model */
                var table = new Hashtable(GetItemCount());
                if (value != null)
                {
                    foreach (var x in value)
                    {
                        table[x] = true;
                    }
                }
                foreach (var x in Objects)
                {
                    SetObjectCheckedness(x, table.ContainsKey(x) ? CheckState.Checked : CheckState.Unchecked);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IEnumerable CheckedObjectsEnumerable
        {
            get { return CheckedObjects; }
            set { CheckedObjects = EnumerableToArray(value, true); }
        }

        [Editor("libolv.OlvColumnCollectionEditor", "System.Drawing.Design.UITypeEditor")]
        public new ColumnHeaderCollection Columns
        {
            get { return base.Columns; }
        }

        [Browsable(false), Obsolete("Use GetFilteredColumns() and OlvColumn.IsTileViewColumn instead"),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<OlvColumn> ColumnsForTileView
        {
            get { return GetFilteredColumns(View.Tile); }
        }

        [Browsable(false)]
        public virtual List<OlvColumn> ColumnsInDisplayOrder
        {
            get
            {
                var columnsInDisplayOrder = new OlvColumn[Columns.Count];
                foreach (OlvColumn col in Columns)
                {
                    columnsInDisplayOrder[col.DisplayIndex] = col;
                }
                return new List<OlvColumn>(columnsInDisplayOrder);
            }
        }

        [Browsable(false)]
        public Rectangle ContentRectangle
        {
            get
            {
                var r = ClientRectangle;
                /* If the listview has a header control, remove the header from the control area */
                if ((View == View.Details || ShowHeaderInAllViews) && HeaderControl != null)
                {
                    var hdrBounds = new Rectangle();
                    NativeMethods.GetClientRect(HeaderControl.Handle, ref hdrBounds);
                    r.Y = hdrBounds.Height;
                    r.Height = r.Height - hdrBounds.Height;
                }
                return r;
            }
        }

        [Category("ObjectListView"),
         Description("Should the control copy the selection to the clipboard when the user presses Ctrl-C?"),
         DefaultValue(true)]
        public virtual bool CopySelectionOnControlC
        {
            get { return _copySelectionOnControlC; }
            set { _copySelectionOnControlC = value; }
        }

        [Category("ObjectListView"),
         Description("Should the Ctrl-C copy process use the DragSource to create the Clipboard data object?"),
         DefaultValue(true)]
        public bool CopySelectionOnControlCUsesDragSource
        {
            get { return _copySelectionOnControlCUsesDragSource; }
            set { _copySelectionOnControlCUsesDragSource = value; }
        }

        [Browsable(false)]
        protected IList<IDecoration> Decorations
        {
            get { return _decorations; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IRenderer DefaultRenderer
        {
            get { return _defaultRenderer; }
            set { _defaultRenderer = value ?? new BaseRenderer(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDragSource DragSource { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDropSink DropSink
        {
            get { return _dropSink; }
            set
            {
                if (_dropSink == value)
                {
                    return;
                }
                /* Stop listening for events on the old sink */
                var oldSink = _dropSink as SimpleDropSink;
                if (oldSink != null)
                {
                    oldSink.CanDrop -= DropSinkCanDrop;
                    oldSink.Dropped -= DropSinkDropped;
                    oldSink.ModelCanDrop -= DropSinkModelCanDrop;
                    oldSink.ModelDropped -= DropSinkModelDropped;
                }
                _dropSink = value;
                AllowDrop = (value != null);
                if (_dropSink != null)
                {
                    _dropSink.ListView = this;
                }
                /* Start listening for events in the new sink */
                var newSink = value as SimpleDropSink;
                if (newSink == null)
                {
                    return;
                }
                newSink.CanDrop += DropSinkCanDrop;
                newSink.Dropped += DropSinkDropped;
                newSink.ModelCanDrop += DropSinkModelCanDrop;
                newSink.ModelDropped += DropSinkModelDropped;
            }
        }

        /* Forward events from the drop sink to the control itself */

        private void DropSinkCanDrop(object sender, OlvDropEventArgs e)
        {
            OnCanDrop(e);
        }

        private void DropSinkDropped(object sender, OlvDropEventArgs e)
        {
            OnDropped(e);
        }

        private void DropSinkModelCanDrop(object sender, ModelDropEventArgs e)
        {
            OnModelCanDrop(e);
        }

        private void DropSinkModelDropped(object sender, ModelDropEventArgs e)
        {
            OnModelDropped(e);
        }

        [Category("ObjectListView"), Description("When the list has no items, show this message in the control"),
         DefaultValue(null), Localizable(true)]
        public virtual String EmptyListMsg
        {
            get
            {
                var overlay = EmptyListMsgOverlay as TextOverlay;
                return overlay == null ? null : overlay.Text;
            }
            set
            {
                var overlay = EmptyListMsgOverlay as TextOverlay;
                if (overlay == null)
                {
                    return;
                }
                overlay.Text = value;
                Invalidate();
            }
        }

        [Category("ObjectListView"), Description("What font should the 'list empty' message be drawn in?"),
         DefaultValue(null)]
        public virtual Font EmptyListMsgFont
        {
            get
            {
                var overlay = EmptyListMsgOverlay as TextOverlay;
                return overlay == null ? null : overlay.Font;
            }
            set
            {
                var overlay = EmptyListMsgOverlay as TextOverlay;
                if (overlay != null)
                {
                    overlay.Font = value;
                }
            }
        }

        [Browsable(false)]
        public virtual Font EmptyListMsgFontOrDefault
        {
            get { return EmptyListMsgFont ?? new Font("Tahoma", 14); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IOverlay EmptyListMsgOverlay
        {
            get { return _emptyListMsgOverlay; }
            set
            {
                if (_emptyListMsgOverlay == value)
                {
                    return;
                }
                _emptyListMsgOverlay = value;
                Invalidate();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IEnumerable FilteredObjects
        {
            get { return IsFiltering ? FilterObjects(Objects, ModelFilter, ListFilter) : Objects; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FilterMenuBuilder FilterMenuBuildStrategy
        {
            get { return _filterMenuBuilder; }
            set { _filterMenuBuilder = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ListViewGroupCollection Groups
        {
            get { return base.Groups; }
        }

        [Category("ObjectListView"), Description("The image list from which group header will take their images"),
         DefaultValue(null)]
        public ImageList GroupImageList
        {
            get { return _groupImageList; }
            set
            {
                _groupImageList = value;
                if (Created)
                {
                    NativeMethods.SetGroupImageList(this, value);
                }
            }
        }

        [Category("ObjectListView"), Description("The format to use when suffixing item counts to group titles"),
         DefaultValue(null), Localizable(true)]
        public virtual string GroupWithItemCountFormat { get; set; }

        [Browsable(false)]
        public virtual string GroupWithItemCountFormatOrDefault
        {
            get
            {
                return string.IsNullOrEmpty(GroupWithItemCountFormat)
                           ? "{0} [{1} items]"
                           : GroupWithItemCountFormat;
            }
        }

        [Category("ObjectListView"), Description("The format to use when suffixing item counts to group titles"),
         DefaultValue(null), Localizable(true)]
        public virtual string GroupWithItemCountSingularFormat { get; set; }

        [Browsable(false)]
        public virtual string GroupWithItemCountSingularFormatOrDefault
        {
            get
            {
                return string.IsNullOrEmpty(GroupWithItemCountSingularFormat)
                           ? "{0} [{1} item]"
                           : GroupWithItemCountSingularFormat;
            }
        }

        [Browsable(true), Category("ObjectListView"),
         Description("Should the groups in this control be collapsible (Vista and later only)."), DefaultValue(true)]
        public bool HasCollapsibleGroups { get; set; }

        [Browsable(false)]
        public virtual bool HasEmptyListMsg
        {
            get { return !string.IsNullOrEmpty(EmptyListMsg); }
        }

        [Browsable(false)]
        public bool HasOverlays
        {
            get
            {
                return (Overlays.Count > 2 ||
                        _imageOverlay.Image != null ||
                        !string.IsNullOrEmpty(_textOverlay.Text));
            }
        }

        [Browsable(false)]
        public HeaderControl HeaderControl
        {
            get { return _headerControl ?? (_headerControl = new HeaderControl(this)); }
        }

        [DefaultValue(null)]
        [Browsable(false)]
        [Obsolete("Use a HeaderFormatStyle instead", false)]
        public Font HeaderFont
        {
            get { return HeaderFormatStyle == null ? null : HeaderFormatStyle.Normal.Font; }
            set
            {
                if (value == null && HeaderFormatStyle == null)
                {
                    return;
                }
                if (HeaderFormatStyle == null)
                {
                    HeaderFormatStyle = new HeaderFormatStyle();
                }
                HeaderFormatStyle.SetFont(value);
            }
        }

        [Category("ObjectListView"), Description("What style will be used to draw the control's header"),
         DefaultValue(null)]
        public HeaderFormatStyle HeaderFormatStyle { get; set; }

        [Category("ObjectListView"), Description("What is the maximum height of the header? -1 means no maximum"),
         DefaultValue(-1)]
        public int HeaderMaximumHeight
        {
            get { return _headerMaximumHeight; }
            set { _headerMaximumHeight = value; }
        }

        [Category("ObjectListView"), Description("Will the column headers be drawn strictly according to OS theme?"),
         DefaultValue(true)]
        public bool HeaderUsesThemes
        {
            get { return _headerUsesThemes; }
            set { _headerUsesThemes = value; }
        }

        [Category("ObjectListView"), Description("Will the text of the column headers be word wrapped?"),
         DefaultValue(false)]
        public bool HeaderWordWrap
        {
            get { return _headerWordWrap; }
            set
            {
                _headerWordWrap = value;
                if (_headerControl != null)
                {
                    _headerControl.WordWrap = value;
                }
            }
        }

        [Browsable(false)]
        public ToolTipControl HeaderToolTip
        {
            get { return HeaderControl.ToolTip; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int HotRowIndex { get; protected set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int HotColumnIndex { get; protected set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual HitTestLocation HotCellHitLocation { get; protected set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual HitTestLocationEx HotCellHitLocationEx { get; protected set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OlvGroup HotGroup { get; internal set; }

        [Browsable(false), Obsolete("Use HotRowIndex instead", false)]
        public virtual int HotItemIndex
        {
            get { return HotRowIndex; }
        }

        [Category("ObjectListView"), Description("How should the row under the cursor be highlighted"),
         DefaultValue(null)]
        public virtual HotItemStyle HotItemStyle
        {
            get { return _hotItemStyle; }
            set
            {
                if (HotItemStyle != null)
                {
                    RemoveOverlay(HotItemStyle.Overlay);
                }
                _hotItemStyle = value;
                if (HotItemStyle != null)
                {
                    AddOverlay(HotItemStyle.Overlay);
                }
            }
        }

        [Category("ObjectListView"), Description("How should hyperlinks be drawn"), DefaultValue(null)]
        public virtual HyperlinkStyle HyperlinkStyle { get; set; }

        [Category("ObjectListView"),
         Description("The background foregroundColor of selected rows when the control is owner drawn"),
         DefaultValue(typeof (Color), "")]
        public virtual Color HighlightBackgroundColor
        {
            get { return _highlightBackgroundColor; }
            set { _highlightBackgroundColor = value; }
        }

        [Browsable(false)]
        public virtual Color HighlightBackgroundColorOrDefault
        {
            get { return HighlightBackgroundColor.IsEmpty ? SystemColors.Highlight : HighlightBackgroundColor; }
        }

        [Category("ObjectListView"),
         Description("The foreground foregroundColor of selected rows when the control is owner drawn"),
         DefaultValue(typeof (Color), "")]
        public virtual Color HighlightForegroundColor
        {
            get { return _highlightForegroundColor; }
            set { _highlightForegroundColor = value; }
        }

        [Browsable(false)]
        public virtual Color HighlightForegroundColorOrDefault
        {
            get { return HighlightForegroundColor.IsEmpty ? SystemColors.HighlightText : HighlightForegroundColor; }
        }

        [Category("ObjectListView"), Description(
            "When rows are copied or dragged, will data in hidden columns be included in the text? If this is false, only visible columns will be included."
            ), DefaultValue(false)]
        public virtual bool IncludeHiddenColumnsInDataTransfer { get; set; }

        [Category("ObjectListView"), Description("When rows are copied, will column headers be in the text?."),
         DefaultValue(false)]
        public virtual bool IncludeColumnHeadersInCopy { get; set; }

        [Browsable(false)]
        public virtual bool IsCellEditing
        {
            get { return CellEditor != null; }
        }

        [Browsable(false)]
        public virtual bool IsDesignMode
        {
            get { return DesignMode; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool IsFiltering
        {
            get { return UseFiltering && (ModelFilter != null || ListFilter != null); }
        }

        [Category("ObjectListView"),
         Description(
             "When the user types into a list, should the values in the current sort column be searched to find a match?"
             ),
         DefaultValue(true)]
        public virtual bool IsSearchOnSortColumn
        {
            get { return _isSearchOnSortColumn; }
            set { _isSearchOnSortColumn = value; }
        }



        /// <summary>
        /// Gets or sets if this control will use a SimpleDropSink to receive drops
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting this replaces any previous DropSink.
        /// </para>
        /// <para>
        /// After setting this to true, the SimpleDropSink will still need to be configured
        /// to say when it can accept drops and what should happen when something is dropped.
        /// The need to do these things makes this property mostly useless :(
        /// </para>
        /// </remarks>
        [Category("ObjectListView"),
         Description("Should this control will use a SimpleDropSink to receive drops."),
         DefaultValue(false)]
        public virtual bool IsSimpleDropSink
        {
            get { return DropSink != null; }
            set { DropSink = value ? new SimpleDropSink() : null; }
        }

        /// <summary>
        /// Gets or sets if this control will use a SimpleDragSource to initiate drags
        /// </summary>
        /// <remarks>Setting this replaces any previous DragSource</remarks>
        [Category("ObjectListView"),
         Description("Should this control use a SimpleDragSource to initiate drags out from this control"),
         DefaultValue(false)]
        public virtual bool IsSimpleDragSource
        {
            get { return DragSource != null; }
            set { DragSource = value ? new SimpleDragSource() : null; }
        }

        /// <summary>
        /// Hide the Items collection so it's not visible in the Properties grid.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ListViewItemCollection Items
        {
            get { return base.Items; }
        }

        [Category("ObjectListView"),
         Description("The owner drawn renderer that draws items when the list is in non-Details view."),
         DefaultValue(null)]
        public IRenderer ItemRenderer { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual OlvColumn LastSortColumn
        {
            get { return PrimarySortColumn; }
            set { PrimarySortColumn = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual SortOrder LastSortOrder
        {
            get { return PrimarySortOrder; }
            set { PrimarySortOrder = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IListFilter ListFilter
        {
            get { return _listFilter; }
            set
            {
                _listFilter = value;
                if (UseFiltering)
                {
                    UpdateFiltering();
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IModelFilter ModelFilter
        {
            get { return _modelFilter; }
            set
            {
                _modelFilter = value;
                if (UseFiltering)
                {
                    UpdateFiltering();
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual OlvListViewHitTestInfo MouseMoveHitTest { get; private set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList<OlvGroup> OlvGroups { get; set; }

        [Category("ObjectListView"), Description("Should the DrawColumnHeader event be triggered"), DefaultValue(false)]
        public bool OwnerDrawnHeader { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IEnumerable Objects
        {
            get { return _objects; }
            set { SetObjects(value, true); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IEnumerable ObjectsForClustering
        {
            get { return Objects; }
        }

        [Category("ObjectListView"), Description("The image that will be drawn over the top of the ListView"),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ImageOverlay OverlayImage
        {
            get { return _imageOverlay; }
            set
            {
                if (_imageOverlay == value)
                {
                    return;
                }
                RemoveOverlay(_imageOverlay);
                _imageOverlay = value;
                AddOverlay(_imageOverlay);
            }
        }

        [Category("ObjectListView"), Description("The text that will be drawn over the top of the ListView"),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TextOverlay OverlayText
        {
            get { return _textOverlay; }
            set
            {
                if (_textOverlay == value)
                {
                    return;
                }
                RemoveOverlay(_textOverlay);
                _textOverlay = value;
                AddOverlay(_textOverlay);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int OverlayTransparency
        {
            get { return _overlayTransparency; }
            set { _overlayTransparency = Math.Min(255, Math.Max(0, value)); }
        }

        [Browsable(false)]
        protected IList<IOverlay> Overlays
        {
            get { return _overlays; }
        }

        [Category("ObjectListView"), Description("Will primary checkboxes persistent their values across list rebuilds")
        , DefaultValue(true)]
        public virtual bool PersistentCheckBoxes
        {
            get { return _persistentCheckBoxes; }
            set
            {
                if (_persistentCheckBoxes == value)
                {
                    return;
                }
                _persistentCheckBoxes = value;
                ClearPersistentCheckState();
            }
        }

        protected Dictionary<Object, CheckState> CheckStateMap
        {
            get { return _checkStateMap ?? (_checkStateMap = new Dictionary<object, CheckState>()); }
            set { _checkStateMap = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual OlvColumn PrimarySortColumn
        {
            get { return _primarySortColumn; }
            set
            {
                _primarySortColumn = value;
                if (TintSortColumn)
                {
                    SelectedColumn = value;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual SortOrder PrimarySortOrder { get; set; }

        [Category("ObjectListView"), Description("Should non-editable checkboxes be drawn as disabled?"),
         DefaultValue(false)]
        public virtual bool RenderNonEditableCheckboxesAsDisabled { get; set; }

        [Category("ObjectListView"),
         Description("Specify the height of each row in pixels. -1 indicates default height"), DefaultValue(-1)]
        public virtual int RowHeight
        {
            get { return _rowHeight; }
            set
            {
                if (value < 1)
                {
                    _rowHeight = -1;
                }
                else
                {
                    _rowHeight = value;
                }
                if (DesignMode)
                {
                    return;
                }
                SetupBaseImageList();
                if (CheckBoxes)
                {
                    InitializeStateImageList();
                }
            }
        }

        [Browsable(false)]
        public virtual int RowHeightEffective
        {
            get
            {
                switch (View)
                {
                    case View.List:
                    case View.SmallIcon:
                    case View.Details:
                        return Math.Max(SmallImageSize.Height, Font.Height);

                    case View.Tile:
                        return TileSize.Height;

                    case View.LargeIcon:
                        if (LargeImageList == null)
                            return Font.Height;

                        return Math.Max(LargeImageList.ImageSize.Height, Font.Height);

                    default:
                        /* This should never happen */
                        return 0;
                }
            }
        }

        [Browsable(false)]
        public virtual int RowsPerPage
        {
            get { return NativeMethods.GetCountPerPage(this); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual OlvColumn SecondarySortColumn { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual SortOrder SecondarySortOrder
        {
            get { return _secondarySortOrder; }
            set { _secondarySortOrder = value; }
        }

        [Category("ObjectListView"), Description("Should the control select all rows when the user presses Ctrl-A?"),
         DefaultValue(true)]
        public virtual bool SelectAllOnControlA
        {
            get { return _selectAllOnControlA; }
            set { _selectAllOnControlA = value; }
        }

        [Category("ObjectListView"),
         Description(
             "When the user right clicks on the column headers, should a menu be presented which will allow them to choose which columns will be shown in the view?"
             ),
         DefaultValue(true)]
        public virtual bool SelectColumnsOnRightClick
        {
            get { return SelectColumnsOnRightClickBehaviour != ColumnSelectBehaviour.None; }
            set
            {
                if (value)
                {
                    if (SelectColumnsOnRightClickBehaviour == ColumnSelectBehaviour.None)
                    {
                        SelectColumnsOnRightClickBehaviour = ColumnSelectBehaviour.InlineMenu;
                    }
                }
                else
                {
                    SelectColumnsOnRightClickBehaviour = ColumnSelectBehaviour.None;
                }
            }
        }

        [Category("ObjectListView"),
         Description("When the user right clicks on the column headers, how will the user be able to select columns?"),
         DefaultValue(ColumnSelectBehaviour.InlineMenu)]
        public virtual ColumnSelectBehaviour SelectColumnsOnRightClickBehaviour
        {
            get { return _selectColumnsOnRightClickBehaviour; }
            set { _selectColumnsOnRightClickBehaviour = value; }
        }

        [Category("ObjectListView"),
         Description("When the column select inline menu is open, should it stay open after an item is selected?"),
         DefaultValue(true)]
        public virtual bool SelectColumnsMenuStaysOpen
        {
            get { return _selectColumnsMenuStaysOpen; }
            set { _selectColumnsMenuStaysOpen = value; }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OlvColumn SelectedColumn
        {
            get { return _selectedColumn; }
            set
            {
                _selectedColumn = value;
                if (value == null)
                {
                    RemoveDecoration(_selectedColumnDecoration);
                }
                else
                {
                    if (!HasDecoration(_selectedColumnDecoration))
                    {
                        AddDecoration(_selectedColumnDecoration);
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IDecoration SelectedRowDecoration { get; set; }

        [Category("ObjectListView"),
         Description("The color that will be used to tint the selected column"),
         DefaultValue(typeof (Color), "")]
        public virtual Color SelectedColumnTint
        {
            get { return _selectedColumnTint; }
            set
            {
                _selectedColumnTint = value.A == 255 ? Color.FromArgb(15, value) : value;
                _selectedColumnDecoration.Tint = _selectedColumnTint;
            }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int SelectedIndex
        {
            get { return SelectedIndices.Count == 1 ? SelectedIndices[0] : -1; }
            set
            {
                SelectedIndices.Clear();
                if (value >= 0 && value < Items.Count)
                {
                    SelectedIndices.Add(value);
                }
            }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual OlvListItem SelectedItem
        {
            get { return SelectedIndices.Count == 1 ? GetItem(SelectedIndices[0]) : null; }
            set
            {
                SelectedIndices.Clear();
                if (value != null)
                {
                    SelectedIndices.Add(value.Index);
                }
            }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Object SelectedObject
        {
            get { return SelectedIndices.Count == 1 ? GetModelObject(SelectedIndices[0]) : null; }
            set
            {
                /* If the given model is already selected, don't do anything else (prevents any flicker) */
                var selectedObject = SelectedObject;
                if (selectedObject != null && selectedObject.Equals(value))
                {
                    return;
                }
                SelectedIndices.Clear();
                SelectObject(value, true);
            }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IList SelectedObjects
        {
            get
            {
                var list = new ArrayList();
                foreach (int index in SelectedIndices)
                {
                    list.Add(GetModelObject(index));
                }
                return list;
            }
            set
            {
                SelectedIndices.Clear();
                SelectObjects(value);
            }
        }

        [Category("ObjectListView"), Description(
            "When the user right clicks on the column headers, should a menu be presented which will allow them to perform common tasks on the listview?"
            ), DefaultValue(false)]
        public virtual bool ShowCommandMenuOnRightClick { get; set; }

        [Category("ObjectListView"),
         Description("If this is true, right clicking on a column header will show a Filter menu option"),
         DefaultValue(true)]
        public bool ShowFilterMenuOnRightClick
        {
            get { return _showFilterMenuOnRightClick; }
            set { _showFilterMenuOnRightClick = value; }
        }

        [Category("Appearance"),
         Description("Should the list view show items in groups?"),
         DefaultValue(true)]
        public new virtual bool ShowGroups
        {
            get { return base.ShowGroups; }
            set
            {
                GroupImageList = GroupImageList;
                base.ShowGroups = value;
            }
        }

        [Category("ObjectListView"), Description("Should the list view show sort indicators in the column headers?"), DefaultValue(true)]
        public virtual bool ShowSortIndicators { get; set; }

        [Category("ObjectListView"),
         Description("Should the list view show images on subitems?"),
         DefaultValue(false)]
        public virtual bool ShowImagesOnSubItems
        {
            get { return _showImagesOnSubItems; }
            set
            {
                _showImagesOnSubItems = value;
                if (Created)
                {
                    ApplyExtendedStyles();
                }
                if (value && VirtualMode)
                {
                    OwnerDraw = true;
                }
            }
        }

        [Category("ObjectListView"), Description("Will group titles be suffixed with a count of the items in the group?"), DefaultValue(false)]
        public virtual bool ShowItemCountOnGroups { get; set; }

        [Category("ObjectListView"),
         Description("Will the control will show column headers in all views?"),
         DefaultValue(true)]
        public bool ShowHeaderInAllViews
        {
            get { return _showHeaderInAllViews; }
            set
            {
                if (_showHeaderInAllViews == value)
                {
                    return;
                }
                _showHeaderInAllViews = value;
                /* If the control isn't already created, everything is fine. */
                if (!Created)
                {
                    return;
                }
                /* If the header is being hidden, we have to recreate the control
                 * to remove the style (not sure why this is) */
                if (!_showHeaderInAllViews)
                {
                    RecreateHandle();
                }
                /* Still more complications. The change doesn't become visible until the View is changed */
                if (View == View.Details) { return; }
                var temp = View;
                View = View.Details;
                View = temp;
            }
        }

        public new ImageList SmallImageList
        {
            get { return _shadowedImageList; }
            set
            {
                _shadowedImageList = value;
                if (UseSubItemCheckBoxes)
                {
                    SetupSubItemCheckBoxes();
                }
                SetupBaseImageList();
            }
        }

        [Browsable(false)]
        public virtual Size SmallImageSize
        {
            get { return BaseSmallImageList == null ? new Size(16, 16) : BaseSmallImageList.ImageSize; }
        }

        [Category("ObjectListView"),
         Description(
             "When the listview is grouped, should the items be sorted by the primary column? If this is false, the items will be sorted by the same column as they are grouped."
             ),
         DefaultValue(true)]
        public virtual bool SortGroupItemsByPrimaryColumn
        {
            get { return _sortGroupItemsByPrimaryColumn; }
            set { _sortGroupItemsByPrimaryColumn = value; }
        }

        [Category("ObjectListView"),
         Description("How many pixels of space will be between groups"),
         DefaultValue(0)]
        public virtual int SpaceBetweenGroups
        {
            get { return _spaceBetweenGroups; }
            set
            {
                if (_spaceBetweenGroups == value)
                {
                    return;
                }
                _spaceBetweenGroups = value;
                SetGroupSpacing();
            }
        }
        
        private void SetGroupSpacing()
        {
            if (!IsHandleCreated)
            {
                return;
            }
            var metrics = new NativeMethods.Lvgroupmetrics
                              {
                                  cbSize = ((uint)Marshal.SizeOf(typeof (NativeMethods.Lvgroupmetrics))),
                                  mask = (uint)GroupMetricsMask.LvgmfBordersize,
                                  Bottom = (uint)SpaceBetweenGroups
                              };
            NativeMethods.SetGroupMetrics(this, metrics);
        }

        [Category("ObjectListView"),
         Description("Should the sort column show a slight tinting?"),
         DefaultValue(false)]
        public virtual bool TintSortColumn
        {
            get { return _tintSortColumn; }
            set
            {
                _tintSortColumn = value;
                if (value && PrimarySortColumn != null)
                {
                    SelectedColumn = PrimarySortColumn;
                }
                else
                {
                    SelectedColumn = null;
                }
            }
        }

        [Category("ObjectListView"),
         Description("Should the primary column have a checkbox that behaves as a tri-state checkbox?"),
         DefaultValue(false)]
        public virtual bool TriStateCheckBoxes
        {
            get { return _triStateCheckBoxes; }
            set
            {
                if (_triStateCheckBoxes == value)
                {
                    return;
                }
                _triStateCheckBoxes = value;
                if (value && !CheckBoxes)
                {
                    CheckBoxes = true;
                }
                InitializeStateImageList();
            }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int TopItemIndex
        {
            get { return View == View.Details && IsHandleCreated ? NativeMethods.GetTopIndex(this) : -1; }
            set
            {
                var newTopIndex = Math.Min(value, GetItemCount() - 1);
                if (View != View.Details || newTopIndex < 0)
                {
                    return;
                }
                try
                {
                    TopItem = Items[newTopIndex];
                    /* Setting the TopItem sometimes gives off by one errors,
                     * that (bizarrely) are correct on a second attempt */
                    if (TopItem != null && TopItem.Index != newTopIndex)
                    {
                        TopItem = GetItem(newTopIndex);
                    }
                }
                catch (NullReferenceException)
                {
                    /* There is a bug in the .NET code where setting the TopItem
                     * will sometimes throw null reference exceptions
                     * There is nothing we can do to get around it. */
                }
            }
        }

        [Category("ObjectListView"),
         Description(
             "When resizing a column by dragging its divider, should any space filling columns be resized at each mouse move?"
             ),
         DefaultValue(true)]
        public virtual bool UpdateSpaceFillingColumnsWhenDraggingColumnDivider
        {
            get { return _updateSpaceFillingColumnsWhenDraggingColumnDivider; }
            set { _updateSpaceFillingColumnsWhenDraggingColumnDivider = value; }
        }

        [Category("ObjectListView"),
         Description("The background color of selected rows when the control is owner drawn and doesn't have the focus"),
         DefaultValue(typeof (Color), "")]
        public virtual Color UnfocusedHighlightBackgroundColor
        {
            get { return _unfocusedHighlightBackgroundColor; }
            set { _unfocusedHighlightBackgroundColor = value; }
        }

        [Browsable(false)]
        public virtual Color UnfocusedHighlightBackgroundColorOrDefault
        {
            get
            {
                return UnfocusedHighlightBackgroundColor.IsEmpty
                           ? SystemColors.Control
                           : UnfocusedHighlightBackgroundColor;
            }
        }

        [Category("ObjectListView"),
         Description("The foreground color of selected rows when the control is owner drawn and doesn't have the focus"),
         DefaultValue(typeof (Color), "")]
        public virtual Color UnfocusedHighlightForegroundColor
        {
            get { return _unfocusedHighlightForegroundColor; }
            set { _unfocusedHighlightForegroundColor = value; }
        }

        [Browsable(false)]
        public virtual Color UnfocusedHighlightForegroundColorOrDefault
        {
            get
            {
                return UnfocusedHighlightForegroundColor.IsEmpty
                           ? SystemColors.ControlText
                           : UnfocusedHighlightForegroundColor;
            }
        }

        [Category("ObjectListView"), Description("Should the list view use a different backcolor to alternate rows?"), DefaultValue(false)]
        public virtual bool UseAlternatingBackColors { get; set; }

        [Category("ObjectListView"), Description("Should FormatCell events be triggered to every cell that is built?"), DefaultValue(false)]
        public bool UseCellFormatEvents { get; set; }

        [Category("ObjectListView"),
         Description("Should the selected row be drawn with non-standard foreground and background colors?"),
         DefaultValue(false)]
        public bool UseCustomSelectionColors
        {
            get { return _useCustomSelectionColors; }
            set
            {
                _useCustomSelectionColors = value;
                if (!DesignMode && value)
                {
                    OwnerDraw = true;
                }
            }
        }
  
        [Category("ObjectListView"),
         Description("Should the list use the same hot item and selection mechanism as Vista?"),
         DefaultValue(false)]
        public bool UseExplorerTheme
        {
            get { return _useExplorerTheme; }
            set
            {
                _useExplorerTheme = value;
                if (Created)
                {
                    NativeMethods.SetWindowTheme(Handle, value ? "explorer" : "", null);
                }
            }
        }

        [Category("ObjectListView"),
         Description("Should the list enable filtering?"),
         DefaultValue(false)]
        public virtual bool UseFiltering
        {
            get { return _useFiltering; }
            set
            {
                _useFiltering = value;
                UpdateFiltering();
            }
        }

        [Category("ObjectListView"),
         Description("Should an image be drawn in a column's header when that column is being used for filtering?"),
         DefaultValue(false)]
        public virtual bool UseFilterIndicator
        {
            get { return _useFilterIndicator; }
            set
            {
                if (_useFilterIndicator == value)
                {
                    return;
                }
                _useFilterIndicator = value;
                if (_useFilterIndicator)
                {
                    HeaderUsesThemes = false;
                }
                Invalidate();
            }
        }

        [Category("ObjectListView"),
         Description("Should HotTracking be used? Hot tracking applies special formatting to the row under the cursor"),
         DefaultValue(false)]
        public bool UseHotItem
        {
            get { return _useHotItem; }
            set
            {
                _useHotItem = value;
                if (HotItemStyle == null) { return; }
                if (value)
                {
                    AddOverlay(HotItemStyle.Overlay);
                }
                else
                {
                    RemoveOverlay(HotItemStyle.Overlay);
                }
            }
        }
        
        [Category("ObjectListView"),
         Description("Should hyperlinks be shown on this control?"),
         DefaultValue(false)]
        public bool UseHyperlinks
        {
            get { return _useHyperlinks; }
            set
            {
                _useHyperlinks = value;
                if (value && HyperlinkStyle == null)
                {
                    HyperlinkStyle = new HyperlinkStyle();
                }
            }
        }

        [Category("ObjectListView"),
         Description("Should this control show overlays"),
         DefaultValue(true)]
        public bool UseOverlays
        {
            get { return _useOverlays; }
            set { _useOverlays = value; }
        }

        [Category("ObjectListView"),
         Description("Should this control be configured to show check boxes on subitems."),
         DefaultValue(false)]
        public bool UseSubItemCheckBoxes
        {
            get { return _useSubItemCheckBoxes; }
            set
            {
                _useSubItemCheckBoxes = value;
                if (value)
                {
                    SetupSubItemCheckBoxes();
                }
            }
        }
        
        [Category("ObjectListView"),
         Description("Should the list use a translucent selection mechanism (like Vista)"),
         DefaultValue(false)]
        public bool UseTranslucentSelection
        {
            get { return _useTranslucentSelection; }
            set
            {
                _useTranslucentSelection = value;
                if (value)
                {
                    var rbd = new RowBorderDecoration
                                  {
                                      BorderPen = new Pen(Color.FromArgb(154, 223, 251)),
                                      FillBrush = new SolidBrush(Color.FromArgb(48, 163, 217, 225)),
                                      BoundsPadding = new Size(0, 0),
                                      CornerRounding = 6.0f
                                  };
                    SelectedRowDecoration = rbd;
                }
                else
                {
                    SelectedRowDecoration = null;
                }
            }
        }

        [Category("ObjectListView"),
         Description("Should the list use a translucent hot row highlighting mechanism (like Vista)"),
         DefaultValue(false)]
        public bool UseTranslucentHotItem
        {
            get { return _useTranslucentHotItem; }
            set
            {
                _useTranslucentHotItem = value;
                if (value)
                {
                    HotItemStyle = new HotItemStyle();
                    var rbd = new RowBorderDecoration
                                  {
                                      BorderPen = new Pen(Color.FromArgb(154, 223, 251)),
                                      BoundsPadding = new Size(0, 0),
                                      CornerRounding = 6.0f,
                                      FillGradientFrom = Color.FromArgb(0, 255, 255, 255),
                                      FillGradientTo = Color.FromArgb(64, 183, 237, 240)
                                  };
                    HotItemStyle.Decoration = rbd;
                }
                else
                {
                    HotItemStyle = null;
                }
                UseHotItem = value;
            }
        }

        public new View View
        {
            get { return base.View; }
            set
            {
                if (base.View == value)
                {
                    return;
                }
                if (Frozen)
                {
                    base.View = value;
                    SetupBaseImageList();
                }
                else
                {
                    Freeze();
                    if (value == View.Tile)
                    {
                        CalculateReasonableTileSize();
                    }
                    base.View = value;
                    SetupBaseImageList();
                    Unfreeze();
                }
            }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual BooleanCheckStateGetterDelegate BooleanCheckStateGetter
        {
            set
            {
                if (value == null)
                {
                    CheckStateGetter = null;
                }
                else
                {
                    CheckStateGetter = x => value(x) ? CheckState.Checked : CheckState.Unchecked;
                }
            }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual BooleanCheckStatePutterDelegate BooleanCheckStatePutter
        {
            set
            {
                if (value == null)
                {
                    CheckStatePutter = null;
                }
                else
                {
                    CheckStatePutter = delegate(Object x, CheckState state)
                    {
                        var isChecked = (state == CheckState.Checked);
                        return value(x, isChecked)
                                   ? CheckState.Checked
                                   : CheckState.Unchecked;
                    };
                }
            }
        }

        [Browsable(false)]
        public virtual bool CanShowGroups
        {
            get { return true; }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool CanUseApplicationIdle
        {
            get { return _canUseApplicationIdle; }
            set { _canUseApplicationIdle = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual CellToolTipGetterDelegate CellToolTipGetter { get; set; }

        [Category("ObjectListView"),
         Description("The name of the property or field that holds the 'checkedness' of the model"),
         DefaultValue(null)]
        public virtual string CheckedAspectName
        {
            get { return _checkedAspectName; }
            set
            {
                _checkedAspectName = value;
                if (string.IsNullOrEmpty(_checkedAspectName))
                {
                    _checkedAspectMunger = null;
                    CheckStateGetter = null;
                    CheckStatePutter = null;
                }
                else
                {
                    _checkedAspectMunger = new Munger(_checkedAspectName);
                    CheckStateGetter = delegate(Object modelObject)
                    {
                        var result = _checkedAspectMunger.GetValue(modelObject) as bool?;
                        return result.HasValue
                                   ? (result.Value ? CheckState.Checked : CheckState.Unchecked)
                                   : (TriStateCheckBoxes
                                          ? CheckState.Indeterminate
                                          : CheckState.Unchecked);
                    };
                    CheckStatePutter = delegate(Object modelObject, CheckState newValue)
                    {
                        if (TriStateCheckBoxes && newValue == CheckState.Indeterminate)
                        {
                            _checkedAspectMunger.PutValue(modelObject, null);
                        }
                        else
                        {
                            _checkedAspectMunger.PutValue(modelObject,
                                                          newValue == CheckState.Checked);
                        }
                        return CheckStateGetter(modelObject);
                    };
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual CheckStateGetterDelegate CheckStateGetter { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual CheckStatePutterDelegate CheckStatePutter { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual SortDelegate CustomSorter { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual HeaderToolTipGetterDelegate HeaderToolTipGetter { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual RowFormatterDelegate RowFormatter { get; set; }
    }
}
