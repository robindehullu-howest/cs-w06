using System;
using Newtonsoft.Json;

namespace ExampleExam.Models;

public class Meal
{
    public enum MealType
    {
        NotEatingAtSchool,
        Sandwiches,
        HotMeal
    }

    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("childId")]
    public string ChildId { get; set; }
    [JsonProperty("choice")]
    public MealType Choice { get; set; }
    [JsonProperty("date")]
    public DateTime Date { get; set; }
}