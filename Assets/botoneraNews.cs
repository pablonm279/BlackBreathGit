using UnityEngine;

public class botoneraNews : MonoBehaviour
{
  private const string UrlDiscord = "https://discord.gg/dZTkFGAU4z";
  private const string UrlX = "https://x.com/BlackBreathGame";
  private const string UrlWishlist = "https://store.steampowered.com/app/4227530/The_Black_Breath/";
  private const string UrlForms = "https://docs.google.com/forms/d/e/1FAIpQLScw6OQLZtVQs1ESOWW7UONEKQuufsNh5XB4mi5S2J5wI4sdeQ/viewform";

  private const string UrlYoutube = "https://www.youtube.com/@TheBlackBreathGame";

  public void AbrirDiscord()
  {
    Application.OpenURL(UrlDiscord);
  }

  public void AbrirX()
  {
    Application.OpenURL(UrlX);
  }

  public void AbrirYoutube()
  {
    Application.OpenURL(UrlYoutube);
  }
  public void AbrirWishlist()
  {
    Application.OpenURL(UrlWishlist);
  }
  public void AbrirForms()
  {
    Application.OpenURL(UrlForms);
  }
}
