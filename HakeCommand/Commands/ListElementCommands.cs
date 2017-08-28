using HakeCommand.Framework;
using HakeCommand.Framework.Helpers;
using HakeCommand.Framework.Services.OutputEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HakeCommand.Commands
{
    public sealed class MeasureResult
    {
        public object Sum { get; }
        public object Max { get; }
        public object Min { get; }
        public int Count { get; }

        public MeasureResult(object sum, object max, object min, int count)
        {
            Sum = sum;
            Max = max;
            Min = min;
            Count = count;
        }

        public IOutputInfo OnWrite()
        {
            List<IEnumerable<string>> result = new List<IEnumerable<string>>();
            result.Add(new string[] { nameof(Count), nameof(Sum), nameof(Max), nameof(Min) });
            result.Add(new string[] { Count.ToString(), Sum?.ToString(), Max?.ToString(), Min?.ToString() });
            List<IOutputBody> body = OutputInfo.CreateBodies(result);
            body.Insert(1, OutputInfo.CreateColumnLineSeperator());
            return OutputInfo.Create(null, body, "");
        }
    }
    public sealed class ListElementCommands : CommandSet
    {
        [Description("Get the element at the corresponding index")]
        [Command("at")]
        public object GetElementAt(int index)
        {
            if (InputObject == null)
                SetExceptionAndThrow(new Exception("no input data"));

            Type objectType = InputObject.GetType();
            if (TypeHelper.IsEnumerable(objectType))
            {
                IEnumerable enumerable = (IEnumerable)InputObject;
                IEnumerator enumerator = enumerable.GetEnumerator();
                int c = 0;
                while (enumerator.MoveNext())
                {
                    if (c >= index)
                        return enumerator.Current;
                    c++;
                }
                SetExceptionAndThrow(new Exception("index out of range"));
            }
            SetExceptionAndThrow(new Exception("data is not a set of elements"));
            return null;
        }

        [Description("Get the size of a list of elements")]
        [Command("count")]
        public int GetSetCount()
        {
            if (InputObject == null)
                SetExceptionAndThrow(new Exception("no input data"));
            Type objectType = InputObject.GetType();
            if (TypeHelper.IsEnumerable(objectType))
            {
                IEnumerable enumerable = (IEnumerable)InputObject;
                IEnumerator enumerator = enumerable.GetEnumerator();
                int c = 0;
                while (enumerator.MoveNext())
                {
                    c++;
                }
                return c;
            }
            SetExceptionAndThrow(new Exception("data is not a set of elements"));
            return 0;
        }

        [Description("Get the first element of a list")]
        [Command("first")]
        public object GetFirstElement()
        {
            return GetElementAt(0);
        }

        [Description("Get the last element of a list")]
        [Command("last")]
        public object GetLastElement()
        {
            return GetElementAt(GetSetCount() - 1);
        }

        [Command("measure")]
        public MeasureResult MeasureList(string property)
        {
            if (InputObject == null)
                SetExceptionAndThrow(new Exception("no input data"));

            Type objectType = InputObject.GetType();
            Type elementType = TypeHelper.GetEnumerableElementType(objectType);
            if (elementType == null)
                SetExceptionAndThrow(new Exception("input data should be generic list"));
            object sum = null;
            object min = null;
            object max = null;
            object current;
            bool inited = false;
            int count = 1;
            IEnumerable enumerable = (IEnumerable)InputObject;
            IEnumerator enumerator = enumerable.GetEnumerator();
            if (property == null)
            {
                dynamic value = null;
                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    value = ObjectHelper.TryGetValue(current);
                    if (value != null)
                    {
                        inited = true;
                        break;
                    }
                }
                if (!inited)
                    SetExceptionAndThrow(new Exception($"no valid element inside list"));
                if (value is string str)
                    value = TryConvertFromString(str);
                sum = value;
                min = value;
                max = value;
                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    value = ObjectHelper.TryGetValue(current);
                    if (value == null)
                        continue;
                    if (value is string istr)
                        value = TryConvertFromString(istr);
                    sum = (dynamic)sum + value;
                    if ((dynamic)min > value)
                        min = value;
                    if ((dynamic)max < value)
                        max = value;
                    count++;
                }
            }
            else
            {
                dynamic value = null;
                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    current = ObjectHelper.TryGetValue(current);
                    value = ObjectHelper.GetPropertyByName(current, property, true);
                    if (value != null)
                    {
                        inited = true;
                        break;
                    }
                }
                if (!inited)
                    SetExceptionAndThrow(new Exception($"property {property} does not exist"));
                if (value is string str)
                    value = TryConvertFromString(str);
                sum = value;
                min = value;
                max = value;
                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    current = ObjectHelper.TryGetValue(current);
                    value = ObjectHelper.GetPropertyByName(current, property, true);
                    if (value == null)
                        continue;
                    if (value is string istr)
                        value = TryConvertFromString(istr);
                    sum = (dynamic)sum + value;
                    if ((dynamic)min > value)
                        min = value;
                    if ((dynamic)max < value)
                        max = value;
                    count++;
                }
            }

            return new MeasureResult(sum, max, min, count);
        }
        private object TryConvertFromString(string value)
        {
            /*
            if (sbyte.TryParse(value, out sbyte sbyteValue))
                return sbyteValue;
            if (char.TryParse(value, out char charValue))
                return charValue;
            if (byte.TryParse(value, out byte byteValue))
                return byteValue;
            if (short.TryParse(value, out short shortValue))
                return shortValue;
            if (ushort.TryParse(value, out ushort ushortValue))
                return ushortValue;
            */
            if (int.TryParse(value, out int intValue))
                return intValue;
            if (uint.TryParse(value, out uint uintValue))
                return uintValue;
            if (long.TryParse(value, out long longValue))
                return longValue;
            if (ulong.TryParse(value, out ulong ulongValue))
                return ulongValue;
            if (float.TryParse(value, out float floatValue))
                return floatValue;
            if (double.TryParse(value, out double doubleValue))
                return doubleValue;
            return value;
        }
    }
}
