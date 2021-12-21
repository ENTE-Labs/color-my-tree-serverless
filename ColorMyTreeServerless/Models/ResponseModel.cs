namespace ColorMyTree.Models;

public class ResponseModel
{
    public bool Error { get; set; }
    public string Message { get; set; }
}

public class ResponseModel<T> : ResponseModel
{
    public T Data { get; set; }

    public ResponseModel(T data) => Data = data;
}