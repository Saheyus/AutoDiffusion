namespace Application.Commands
{
    public class CreateImageGenerationCommand : ApplicationCommand
    {
        public CreateImageGenerationCommand(string prompt, string scriptName)
        {
            Prompt = prompt;
            ScriptName = scriptName;
        }

        public string Prompt { get; }
        public string ScriptName { get; }
    }
}
