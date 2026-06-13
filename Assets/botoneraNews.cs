using UnityEngine;

public class botoneraNews : MonoBehaviour
{
  private const string UrlDiscord = "https://discord.gg/sds5NJCWSh";
  private const string UrlX = "https://x.com/BlackBreathGame";
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
}
