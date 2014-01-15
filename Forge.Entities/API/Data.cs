﻿// The MIT License (MIT)
//
// Copyright (c) 2013 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace Forge.Entities {
    public static class Data {
        public interface IData {

            /// <summary>
            /// Return an exact copy of this data instance.
            /// </summary>
            IData Duplicate();
        }

        public abstract class NonVersioned : IData {
            /// <summary>
            /// Return an exact copy of this data instance.
            /// </summary>
            public virtual NonVersioned Duplicate() {
                return (NonVersioned)MemberwiseClone();
            }

            IData IData.Duplicate() {
                return Duplicate();
            }
        }

        public interface IVersioned : IData {
            /// <summary>
            /// Moves all of the data from the specified source into this instance. After this call,
            /// this data instance must be identical to source, such that this instance could
            /// completely replace source in other code and the other code would be unable to tell
            /// the difference.
            /// </summary>
            /// <param name="source">The source to move from.</param>
            void CopyFrom(IVersioned source);

            IVersioned Duplicate();
        }

        public abstract class Versioned<TData> : IVersioned
            where TData : Versioned<TData> {
            public abstract void CopyFrom(TData source);

            void IVersioned.CopyFrom(IVersioned source) {
                CopyFrom((TData)source);
            }

            IData IData.Duplicate() {
                return (IData)MemberwiseClone();
            }

            IVersioned IVersioned.Duplicate() {
                return (IVersioned)MemberwiseClone();
            }
        }

        public interface IConcurrent {
            /// <summary>
            /// This method is called after all modifications have been made during an update have
            /// been made to the data instance. The purpose is to allow for client code to resolve
            /// multiple modifications so that modA(modB(entity)) == modB(modA(entity)).
            /// </summary>
            /// <remarks>
            /// No other calls will be made to the data instance while this function is being
            /// executed.
            /// </remarks>
            void ResolveConcurrentModifications();
        }

        public abstract class ConcurrentVersioned<TData> : Versioned<TData>, IConcurrent
            where TData : ConcurrentVersioned<TData> {
            /// <summary>
            /// This method is called after all modifications have been made during an update have
            /// been made to the data instance. The purpose is to allow for client code to resolve
            /// multiple modifications so that modA(modB(entity)) == modB(modA(entity)).
            /// </summary>
            /// <remarks>
            /// No other calls will be made to the data instance while this function is being
            /// executed.
            /// </remarks>
            public abstract void ResolveConcurrentModifications();
        }

        public abstract class ConcurrentNonVersioned : NonVersioned, IConcurrent {
            /// <summary>
            /// This method is called after all modifications have been made during an update have
            /// been made to the data instance. The purpose is to allow for client code to resolve
            /// multiple modifications so that modA(modB(entity)) == modB(modA(entity)).
            /// </summary>
            /// <remarks>
            /// No other calls will be made to the data instance while this function is being
            /// executed.
            /// </remarks>
            public abstract void ResolveConcurrentModifications();
        }
    }
}