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

using System.Collections.Generic;

namespace Forge.Entities {
    /// <summary>
    /// An ITemplateGroup is simply a collection of templates that IGameSnapshots use.
    /// </summary>
    public interface ITemplateGroup {
        /// <summary>
        /// All of the templates that are within the group.
        /// </summary>
        IEnumerable<ITemplate> Templates { get; }

        /// <summary>
        /// Creates a new ITemplate instance that is attached to this snapshot.
        /// </summary>
        ITemplate CreateTemplate();

        /// <summary>
        /// Attempts to remove the given template from the template group. Be careful that no
        /// IGameSnapshots are referencing the given template.
        /// </summary>
        /// <returns>True if the template was found and removed, false if it was not
        /// found.</returns>
        bool RemoveTemplate(ITemplate template);
    }
}