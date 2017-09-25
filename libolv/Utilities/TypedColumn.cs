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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using libolv.Implementation.Munger;

namespace libolv.Utilities
{
    public class TypedColumn<T> where T : class
    {
        public delegate Object TypedAspectGetterDelegate(T rowObject);
        public delegate void TypedAspectPutterDelegate(T rowObject, Object newValue);
        public delegate Object TypedGroupKeyGetterDelegate(T rowObject);
        public delegate Object TypedImageGetterDelegate(T rowObject);

        private readonly OlvColumn _column;
        private TypedAspectGetterDelegate _aspectGetter;
        private TypedAspectPutterDelegate _aspectPutter;
        private TypedImageGetterDelegate _imageGetter;
        private TypedGroupKeyGetterDelegate _groupKeyGetter;

        public TypedColumn(OlvColumn column)
        {
            _column = column;
        }

        public TypedAspectGetterDelegate AspectGetter
        {
            get { return _aspectGetter; }
            set
            {
                _aspectGetter = value;
                if (value == null)
                {
                    _column.AspectGetter = null;
                }
                else
                {
                    _column.AspectGetter = x => x == null ? null : _aspectGetter((T)x);
                }
            }
        }

        public TypedAspectPutterDelegate AspectPutter
        {
            get { return _aspectPutter; }
            set
            {
                _aspectPutter = value;
                if (value == null)
                {
                    _column.AspectPutter = null;
                }
                else
                {
                    _column.AspectPutter = (x, newValue) => _aspectPutter((T)x, newValue);
                }
            }
        }

        public TypedImageGetterDelegate ImageGetter
        {
            get { return _imageGetter; }
            set
            {
                _imageGetter = value;
                if (value == null)
                {
                    _column.ImageGetter = null;
                }
                else
                {
                    _column.ImageGetter = x => _imageGetter((T)x);
                }
            }
        }

        public TypedGroupKeyGetterDelegate GroupKeyGetter
        {
            get { return _groupKeyGetter; }
            set
            {
                _groupKeyGetter = value;
                if (value == null)
                {
                    _column.GroupKeyGetter = null;
                }
                else
                {
                    _column.GroupKeyGetter = x => _groupKeyGetter((T)x);
                }
            }
        }

        /* Dynamic methods */
        public void GenerateAspectGetter()
        {
            if (!string.IsNullOrEmpty(_column.AspectName))
            {
                AspectGetter = GenerateAspectGetter(typeof (T), _column.AspectName);
            }
        }

        private TypedAspectGetterDelegate GenerateAspectGetter(Type type, string path)
        {
            var getter = new DynamicMethod(string.Empty, typeof(Object),
                new[]
                    {
                        type
                    },
                    type, true);
            GenerateIl(type, path, getter.GetILGenerator());
            return (TypedAspectGetterDelegate)getter.CreateDelegate(typeof(TypedAspectGetterDelegate));
        }

        private void GenerateIl(Type type, string path, ILGenerator il)
        {
            /* Push our model object onto the stack */
            il.Emit(OpCodes.Ldarg_0);
            /* Generate the IL to access each part of the dotted chain */
            var parts = path.Split('.');
            for (var i = 0; i < parts.Length; i++)
            {
                type = GeneratePart(il, type, parts[i], (i == parts.Length - 1));
                if (type == null)
                {
                    break;
                }
            }
            /* If the object to be returned is a value type (e.g. int, bool), it
             * must be boxed, since the delegate returns an Object */
            if (type != null && type.IsValueType && !typeof(T).IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
            il.Emit(OpCodes.Ret);
        }

        private static Type GeneratePart(ILGenerator il, Type type, string pathPart, bool isLastPart)
        {            
            /* Find the first member with the given nam that is a field, property, or parameter-less method */
            var infos = new List<MemberInfo>(type.GetMember(pathPart));
            var info = infos.Find(delegate(MemberInfo x)
                                      {
                                          if (x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property)
                                          {
                                              return true;
                                          }
                                          if (x.MemberType == MemberTypes.Method)
                                          {
                                              return ((MethodInfo)x).GetParameters().Length == 0;
                                          }
                                          return false;
                                      });
            /* If we couldn't find anything with that name, pop the current result and return an error */
            if (info == null)
            {
                il.Emit(OpCodes.Pop);
                if (Munger.IgnoreMissingAspects)
                {
                    il.Emit(OpCodes.Ldnull);
                }
                else
                {
                    il.Emit(OpCodes.Ldstr,
                            string.Format("'{0}' is not a parameter-less method, property or field of type '{1}'",
                                          pathPart, type.FullName));
                }
                return null;
            }
            /* Generate the correct IL to access the member. We remember the type of object that is going to be returned
             * so that we can do a method lookup on it at the next iteration */
            Type resultType = null;
            switch (info.MemberType)
            {
                case MemberTypes.Method:
                    var mi = (MethodInfo)info;
                    il.Emit(mi.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, mi);
                    resultType = mi.ReturnType;
                    break;

                case MemberTypes.Property:
                    var pi = (PropertyInfo)info;
                    il.Emit(OpCodes.Call, pi.GetGetMethod());
                    resultType = pi.PropertyType;
                    break;

                case MemberTypes.Field:
                    var fi = (FieldInfo)info;
                    il.Emit(OpCodes.Ldfld, fi);
                    resultType = fi.FieldType;
                    break;
            }
            /* If the method returned a value type, and something is going to call a method on that value,
             * we need to load its address onto the stack, rather than the object itself. */
            if (resultType != null && (resultType.IsValueType && !isLastPart))
            {
                var lb = il.DeclareLocal(resultType);
                il.Emit(OpCodes.Stloc, lb);
                il.Emit(OpCodes.Ldloca, lb);
            }
            return resultType;
        }
    }
}
