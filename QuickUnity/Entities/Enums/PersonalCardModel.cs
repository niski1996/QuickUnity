using QuickUnity.Data.Tables;

namespace QuickUnity.Entities.Enums;

public class PersonalCardModel
{
    public string Name { get;  }
    public string LastName { get;  }
    public string City { get;  } 
    public string Club { get;  } 

    public PersonalCardModel(ProfileRow profile)
    {
        Id = profile.Id;
        Name = profile.Name;
        LastName = profile.LastName;
        City = profile.City;
        Club = profile.Club;
        Video = profile.Videos;
    }

    public ICollection<VideoRow> Video { get; set; }

    public int VideosCount => Video.Count;

    public Guid Id { get; set; }
    
    
}