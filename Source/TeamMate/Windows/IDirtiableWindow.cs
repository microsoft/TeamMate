
namespace Microsoft.Tools.TeamMate.Windows
{
    public interface IDirtiableWindow
    {
        bool NoPrompt { get; set; }

        bool PromptSaveIfDirty();
    }
}
