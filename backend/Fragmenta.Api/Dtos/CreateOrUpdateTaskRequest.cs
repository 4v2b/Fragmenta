﻿using System.ComponentModel.DataAnnotations;

namespace Fragmenta.Api.Dtos
{
    public class CreateOrUpdateTaskRequest
    {
        [MinLength(1)]
        [MaxLength(50)]
        public required string Title { get; set; }
        
        [MaxLength(150)]
        public string? Description { get; set; }
        public required DateTime? DueDate { get; set; }
        public required long? AssigneeId { get; set; }
        public required float Weight { get; set; }
        public required int Priority { get; set; }
        public required List<long> TagsId { get; set; }
    }
}
