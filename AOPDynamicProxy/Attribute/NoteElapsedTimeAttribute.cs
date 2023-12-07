using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
    /// <summary>
    /// 记录方法的耗时
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class NoteElapsedTimeAttribute : Attribute
    {
        /// <summary>
        /// 记录方式(eg:log)
        /// </summary>
        public EnumElapsedTimeNoteMode NoteMode { get; private set; }

        /// <summary>
        /// 在何版本下需记录耗时
        /// </summary>
        public EnumFunctionalVersion NoteVersion { get; set; }

        public NoteElapsedTimeAttribute(EnumElapsedTimeNoteMode noteMode)
        {
            NoteMode = noteMode;
        }
    }
}
