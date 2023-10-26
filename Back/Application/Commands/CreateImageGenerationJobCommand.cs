using Domain.Entities;

namespace Application.Commands
{
    public class CreateImageGenerationJobCommand : ApplicationCommand<ImageGenerationJob>
    {
        public CreateImageGenerationJobCommand(string inputText)
        {
            InputText = inputText;
        }

        public string InputText { get; }
    }
}
