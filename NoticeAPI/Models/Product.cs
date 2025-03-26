﻿namespace NoticeAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string Description { get; set; }
        public int Stock { get; set; }  
        public string ImageUrl { get; set; }
        public bool IsAvaiable => Stock > 0;
    }
}
