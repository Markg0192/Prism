using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Tekla.Structures.ObjectPropertiesLibrary.Properties
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					ResourceManager resourceManager = (resourceMan = new ResourceManager("Tekla.Structures.ObjectPropertiesLibrary.Properties.Resources", typeof(Resources).Assembly));
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal static Bitmap AddTable
		{
			get
			{
				object @object = ResourceManager.GetObject("AddTable", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap autoselect_objects_active_big
		{
			get
			{
				object @object = ResourceManager.GetObject("autoselect_objects_active_big", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Icon autoselect_objects_passive_big
		{
			get
			{
				object @object = ResourceManager.GetObject("autoselect_objects_passive_big", resourceCulture);
				return (Icon)@object;
			}
		}

		internal static Bitmap Cancel
		{
			get
			{
				object @object = ResourceManager.GetObject("Cancel", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap delete_big
		{
			get
			{
				object @object = ResourceManager.GetObject("delete_big", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap EditTableHS
		{
			get
			{
				object @object = ResourceManager.GetObject("EditTableHS", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap EditTableHS1
		{
			get
			{
				object @object = ResourceManager.GetObject("EditTableHS1", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap keep_selection_16
		{
			get
			{
				object @object = ResourceManager.GetObject("keep_selection_16", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap move_down_24
		{
			get
			{
				object @object = ResourceManager.GetObject("move_down_24", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap move_down_241
		{
			get
			{
				object @object = ResourceManager.GetObject("move_down_241", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap open_big
		{
			get
			{
				object @object = ResourceManager.GetObject("open_big", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap PasteHS
		{
			get
			{
				object @object = ResourceManager.GetObject("PasteHS", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap save_as_big
		{
			get
			{
				object @object = ResourceManager.GetObject("save_as_big", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal Resources()
		{
		}
	}
}
