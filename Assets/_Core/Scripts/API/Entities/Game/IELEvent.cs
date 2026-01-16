using System;
using System.Collections.Generic;
using System.Globalization;

[Serializable]
public class IELEvent : IResponse
{
    public string NameToDisplay() => GroupName;

    public string GroupName;
    public int ProfileVersion;
    public UserGroup Group;
    public Role[] Roles;
    public EventData GroupData;

    [Serializable]
    public class UserGroup {
        public string Id, Type, TypeString;
    }

    [Serializable]
    public class Role {
        public string RoleName, RoleId;
    }
}

[Serializable]
public class EventData
{
    public string id;
    public string name;
    public string description;
    public string location;
    public string start_date;
    public string end_date;
    public string banner;
    public string visibility;
    public string created_at;
    public string updated_at;
    public string deleted_at;
    public List<GuestData>  guests;
    
    
    public bool IsPublic => "PUBLIC".Equals(visibility, StringComparison.OrdinalIgnoreCase);
        
    public DateTime StartDateAsDateTime
    {
        get
        {
            if (DateTime.TryParse(start_date, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime result))
            {
                return result;
            }
            return DateTime.MinValue;
        }
    }
        
    public DateTime EndDateAsDateTime
    {
        get
        {
            if (DateTime.TryParse(end_date, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime result))
            {
                return result;
            }
            return DateTime.MinValue;
        }
    }
}

[Serializable]
public class GuestData
{
    public string id;
    public string invited_at;
    public string accepted_at;
    public string guest_id;
    public string event_id;
    public UserData guest;
}

[Serializable]
public class BannerPayload
{
    public string mime;
    public string data;
}

[Serializable]
public class EventPayload
{
    public string name;
    public string description;
    public string location;
    public string start_date;
    public string end_date;
    public string visibility;
    public BannerPayload banner;
}