namespace ChatBlaster.Models;

public class Profile
{
	public int Id { get; set; }
	public string _user_name { get; set; } = null;
	public string _email { get; set; }
	public string _password { get; set; }
	public string _loggedIn { get; set; }
    public string _industry { get; set; }
	public string _companyName { get; set; }
	public string _serviceArea { get; set; }
	public string _companiesPhoneNumber { get; set; }

    public HashSet<string> Conversations { get; set; } = new HashSet<string>();
}
