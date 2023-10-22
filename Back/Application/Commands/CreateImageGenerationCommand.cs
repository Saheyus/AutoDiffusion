using Domain.Entities;

namespace Application.Commands
{
    public class CreateImageGenerationCommand : ApplicationCommand<ImageGeneration>
    {
        public CreateImageGenerationCommand(string prompt)
        {
            Prompt = prompt;
        }

        public string Prompt { get; }
    }
}
