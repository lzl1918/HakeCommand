using HakeCommand.Framework;
using HakeCommand.Framework.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HakeCommand.Commands
{
    public sealed class ListElementCommands : CommandSet
    {
        [DescriptionAttribute("Get the element at the corresponding index")]
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

        [DescriptionAttribute("Get the size of a list of elements")]
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

        [DescriptionAttribute("Get the first element of a list")]
        [Command("first")]
        public object GetFirstElement()
        {
            return GetElementAt(0);
        }

        [DescriptionAttribute("Get the last element of a list")]
        [Command("last")]
        public object GetLastElement()
        {
            return GetElementAt(GetSetCount() - 1);
        }


    }
}
