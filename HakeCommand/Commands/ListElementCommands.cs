using HakeCommand.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HakeCommand.Commands
{
    public sealed class ListElementCommands : CommandSet
    {
        [Statement("Get the element at the corresponding index")]
        [Command("at")]
        public object GetElementAt(int index)
        {
            if (InputObject == null)
                SetExceptionAndThrow(new Exception("no input data"));

            Type interfaceType;
            TypeInfo objectType = InputObject.GetType().GetTypeInfo();
            if ((interfaceType = objectType.GetInterface("System.Collections.IEnumerable")) != null)
            {
                MethodInfo getEnumeratorMethod = interfaceType.GetMethod("GetEnumerator");
                IEnumerator enumerator = (IEnumerator)getEnumeratorMethod.Invoke(InputObject, null);
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

        [Statement("Get the size of a list of elements")]
        [Command("count")]
        public int GetSetCount()
        {
            if (InputObject == null)
                SetExceptionAndThrow(new Exception("no input data"));
            Type interfaceType;
            TypeInfo objectType = InputObject.GetType().GetTypeInfo();
            if ((interfaceType = objectType.GetInterface("System.Collections.IEnumerable")) != null)
            {
                MethodInfo getEnumeratorMethod = interfaceType.GetMethod("GetEnumerator");
                IEnumerator enumerator = (IEnumerator)getEnumeratorMethod.Invoke(InputObject, null);
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

        [Statement("Get the first element of a list")]
        [Command("first")]
        public object GetFirstElement()
        {
            return GetElementAt(0);
        }

        [Statement("Get the last element of a list")]
        [Command("last")]
        public object GetLastElement()
        {
            return GetElementAt(GetSetCount() - 1);
        }


    }
}
