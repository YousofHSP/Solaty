namespace Common;

public class SelectDto(object? id = null, string title = "")
{
        public object Id { get; set; } = id;
        public string Title { get; set; } = title;
        public bool Disabled { get; set; } = false;
}