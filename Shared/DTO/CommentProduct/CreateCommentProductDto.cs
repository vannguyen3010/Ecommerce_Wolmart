﻿using System.ComponentModel.DataAnnotations;

namespace Shared.DTO.CommentProduct
{
    public class CreateCommentProductDto
    {
        public string UserId { get; set; }
        [Required]
        public Guid ProductId { get; set; } // ID sản phẩm được comment
        [Required]
        public string Content { get; set; } // Nội dung comment
    }
   
}
