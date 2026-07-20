namespace API.DTOs.Account
{
    public class facebookResultDto
    {
        public FacebookData Data { get; set; }
    }

    public class FacebookData
    {
        public bool Is_Valid { get; set; }
        public string user_Id { get; set; }
    }
}
