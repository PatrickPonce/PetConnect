using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetConnect.Models; // Asegúrate de que el namespace sea el de tu proyecto

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}