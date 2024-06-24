﻿using System;
using System.ComponentModel.DataAnnotations;

namespace NoteApp.Models
{
    public class Note
    {
        public int NoteId { get; set; }
        [Required(ErrorMessage = "Content is required")]
        [MinLength(1, ErrorMessage = "Content must be at least 1 character")]
        public string? Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
