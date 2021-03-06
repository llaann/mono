//
// MemoryMappedViewStream.cs
//
// Authors:
//	Zoltan Varga (vargaz@gmail.com)
//
// Copyright (C) 2009, Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace System.IO.MemoryMappedFiles
{
	public sealed class MemoryMappedViewStream : UnmanagedMemoryStream {
		IntPtr mmap_handle;
		object monitor;
		long pointerOffset;

		internal MemoryMappedViewStream (IntPtr handle, long offset, long size, MemoryMappedFileAccess access) {
			pointerOffset = offset;
			monitor = new Object ();
			CreateStream (handle, offset, size, access);
		}

		public long PointerOffset
		{
			get { return pointerOffset; }
		}

		public SafeMemoryMappedViewHandle SafeMemoryMappedViewHandle { 
			get {
				throw new NotImplementedException ();
			}
		}

		unsafe void CreateStream (IntPtr handle, long offset, long size, MemoryMappedFileAccess access)
		{
			IntPtr base_address;

			MemoryMapImpl.Map (handle, offset, ref size, access, out mmap_handle, out base_address);

			FileAccess faccess;
			switch (access) {
			case MemoryMappedFileAccess.ReadWrite:
				faccess = FileAccess.ReadWrite;
				break;
			case MemoryMappedFileAccess.Read:
				faccess = FileAccess.Read;
				break;
			case MemoryMappedFileAccess.Write:
				faccess = FileAccess.Write;
				break;
			default:
				throw new NotImplementedException ("access mode " + access + " not supported.");
			}
			Initialize ((byte*)base_address, size, size, faccess);
		}
		 
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			lock (monitor) {
				if (mmap_handle != (IntPtr)(-1)) {
					MemoryMapImpl.Unmap (mmap_handle);
					mmap_handle = (IntPtr)(-1);
				}
			}
		}

		public override void Flush ()
		{
			MemoryMapImpl.Flush (mmap_handle);
		}
	}
}

