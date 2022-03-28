using System.Windows.Forms;

namespace Tekla.Structures
{
	public interface IMainWindow : IWin32Window
	{
		bool IsActive { get; }

		bool IsMinimized { get; }

		void Activate();

		void AttachChildForm(Form form);

		void DetachChildForm(Form form);
	}
}
