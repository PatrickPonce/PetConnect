using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetConnect.Models.Api
{
    public class AnimalApiResponse
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public List<Breed> Breeds { get; set; }
    }

    public class Breed
    {
        public string Name { get; set; }
        public string Origin { get; set; }
        public string Temperament { get; set; }
    }
}