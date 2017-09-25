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
using System.Linq;
using System.Reflection;

namespace libolv.Implementation.Munger
{
    public class SimpleMunger
    {
        private readonly string _aspectName;

        private Type _cachedTargetType;
        private string _cachedName;
        private int _cachedNumberParameters;

        private FieldInfo _resolvedFieldInfo;
        private PropertyInfo _resolvedPropertyInfo;
        private MethodInfo _resolvedMethodInfo;
        private PropertyInfo _indexerPropertyInfo;

        public SimpleMunger(String aspectName)
        {
            _aspectName = aspectName;
        }

        /* Public properties */
        public string AspectName
        {
            get { return _aspectName; }
        }
        
        /* Public interface */
        public Object GetValue(Object target)
        {
            if (target == null)
            {
                return null;
            }
            ResolveName(target, AspectName, 0);
            try
            {
                if (_resolvedPropertyInfo != null)
                {
                    return _resolvedPropertyInfo.GetValue(target, null);
                }
                if (_resolvedMethodInfo != null)
                {
                    return _resolvedMethodInfo.Invoke(target, null);
                }
                if (_resolvedFieldInfo != null)
                {
                    return _resolvedFieldInfo.GetValue(target);
                }
                /* If that didn't work, try to use the indexer property. 
                 * This covers things like dictionaries and DataRows. */
                if (_indexerPropertyInfo != null)
                {
                    return _indexerPropertyInfo.GetValue(target, new object[] {AspectName});
                }
            }
            catch (Exception ex)
            {
                /* Lots of things can do wrong in these invocations */
                throw new MungerException(this, target, ex);
            }
            /* If we get to here, we couldn't find a match for the aspect */
            throw new MungerException(this, target, new MissingMethodException());
        }

        public bool PutValue(object target, object value)
        {
            if (target == null)
            {
                return false;
            }
            ResolveName(target, AspectName, 1);
            try
            {
                if (_resolvedPropertyInfo != null)
                {
                    _resolvedPropertyInfo.SetValue(target, value, null);
                    return true;
                }
                if (_resolvedMethodInfo != null)
                {
                    _resolvedMethodInfo.Invoke(target, new[] { value });
                    return true;
                }
                if (_resolvedFieldInfo != null)
                {
                    _resolvedFieldInfo.SetValue(target, value);
                    return true;
                }
                /* If that didn't work, try to use the indexer property.
                 * This covers things like dictionaries and DataRows. */
                if (_indexerPropertyInfo != null)
                {
                    _indexerPropertyInfo.SetValue(target, value, new object[] { AspectName });
                    return true;
                }
            }
            catch (Exception ex)
            {
                /* Lots of things can do wrong in these invocations */
                throw new MungerException(this, target, ex);
            }
            return false;
        }

        /* Implementation */
        private void ResolveName(object target, string name, int numberMethodParameters)
        {
            if (_cachedTargetType == target.GetType() && _cachedName == name && _cachedNumberParameters == numberMethodParameters)
            {
                return;
            }
            _cachedTargetType = target.GetType();
            _cachedName = name;
            _cachedNumberParameters = numberMethodParameters;
            _resolvedFieldInfo = null;
            _resolvedPropertyInfo = null;
            _resolvedMethodInfo = null;
            _indexerPropertyInfo = null;
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance /*| BindingFlags.NonPublic*/;
            foreach (var pinfo in target.GetType().GetProperties(flags))
            {
                if (pinfo.Name == name)
                {
                    _resolvedPropertyInfo = pinfo;
                    return;
                }
                /* See if we can find an string indexer property while we are here.
                 * We also need to allow for old style <object> keyed collections. */
                if (_indexerPropertyInfo != null || pinfo.Name != "Item") { continue; }
                var par = pinfo.GetGetMethod().GetParameters();
                if (par.Length <= 0) { continue; }
                var parameterType = par[0].ParameterType;
                if (parameterType == typeof(string) || parameterType == typeof(object))
                {
                    _indexerPropertyInfo = pinfo;
                }
            }
            foreach (var info in target.GetType().GetFields(flags).Where(info => info.Name == name))
            {
                _resolvedFieldInfo = info;
                return;
            }
            foreach (var info in target.GetType().GetMethods(flags).Where(info => info.Name == name && info.GetParameters().Length == numberMethodParameters))
            {
                _resolvedMethodInfo = info;
                return;
            }
        }
    }
}
