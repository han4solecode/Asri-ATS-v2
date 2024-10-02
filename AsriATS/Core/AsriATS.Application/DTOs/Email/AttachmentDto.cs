using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Email
{
    public class AttachmentDto
    {
        public string FileName { get; set; }
        public string MimeType { get; set; } // Example: "application/pdf", "image/jpeg"
        public byte[] Content { get; set; }  // The file content as a byte array
    }
}
