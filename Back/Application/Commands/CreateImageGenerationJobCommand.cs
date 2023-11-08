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

        public CreateImageGenerationJobCommand(Guid id, string inputText)
        {
            Id = id;
            InputText = inputText;
        }
    }
}
