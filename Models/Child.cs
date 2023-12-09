using System;
using Newtonsoft.Json;

namespace ExampleExam.Models;

public class Child
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("firstName")]
    public string FirstName { get; set; }
    [JsonProperty("class")]
    public string Class { get; set; }
    [JsonProperty("emailOfParent")]
    public string EmailOfParent { get; set; }
}