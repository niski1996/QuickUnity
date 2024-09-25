namespace QuickUnity.Data.Tables;

public class ProfileRow
{
    public Guid Id {get;set;}
    public Guid UserId {get;set;}
    public DateOnly JoinDate {get;set;}
    public string Name {get;set;}
    public string LastName {get;set;}
    
}