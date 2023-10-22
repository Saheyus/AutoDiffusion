﻿using Domain.Ports;

namespace Domain.Entities
{
    //Aggregate with single entity
    public sealed class ImageGeneration : IAggregate<ImageGeneration>
    {
        public ImageGeneration(Guid id, string prompt)
        {
            Id = id;
            Prompt = !string.IsNullOrWhiteSpace(prompt) ? prompt : throw new ArgumentException("Prompt cannot be null or empty!", nameof(prompt));
            State = ImageGenerationStates.Pending;
        }

        public string Prompt { get; }
        public Guid Id { get; }

        public ImageGeneration Root => this;

        public ImageGenerationStates State { get; private set; }

        public void ChangeState(ImageGenerationStates state)
        {
            State = state;
        }
    }
}