using System;
using System.Web.UI;
using BlogEngine.Core;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using BlogEngine.Core.Providers;
using RecipeDomain;

public partial class SimpleBlog29 : System.Web.UI.MasterPage
{
    private static Regex reg = new Regex(@"(?<=[^])\t{2,}|(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,11}(?=[<])|(?=[\n])\s{2,}");

    protected static string ShRoot = Utils.ApplicationRelativeWebRoot + "editors/tiny_mce_3_5_8/plugins/syntaxhighlighter/";
    public StringBuilder parentNavBuild = new StringBuilder();
    public string strParentID = string.Empty;
    public string strFirstParentID = string.Empty;
    public string pageHeading = string.Empty;
    public string orphanPage = string.Empty;
    
    
    protected void WriteRecipe(RecipeDisplayModel rec)
    {
        pageTitle.Text = rec.RecipeName;
        Title.Text = rec.RecipeName;
        Title2.Text = Title.Text;
        
        
        const string tag = "\n\t<meta name=\"{0}\" content=\"{1} - {2}\" />";
        Page.Header.Controls.Add(new LiteralControl(string.Format(tag, "description", rec.RecipeName, "Parents of food allergic children have shared thousands of their favorite recipes that are indicated as 'free of' many different allergens." )));

        Ingr.Text = GetIngredients(rec.Ingredients);
        Instructs.Text = GetInstructions(rec.Directions);

        rec.SubsitutionNote = FillSubstitution(rec.SubsitutionNote, rec.Ingredients);

        if (!string.IsNullOrEmpty(rec.SubsitutionNote))
        {
            Substitutions.Text = GetSubstitutions(rec.SubsitutionNote);
        }
        else
        {
            SubstitutionSecion.Visible = false;
        }

        ///media/RecipeImages/
        if(rec.Img != null && !string.IsNullOrEmpty(rec.Img.DisplayImageFileName))
        {
            RecImg.ImageUrl = string.Format("/media/RecipeImages/{0}", rec.Img.DisplayImageFileName);
            RecImg.AlternateText = rec.RecipeName;       
            Page.Header.Controls.Add(new LiteralControl(string.Format("\n\t<meta property=\"{0}\" content=\"{1}\" />", "og:image", RecImg.ImageUrl)));
        }
        else
        {
            RecImg.Visible = false;
        }


        SubmitterComment.Text = GetComments(rec.SubmitterComment);

        if (!string.IsNullOrEmpty(rec.ServingSize))
        {
            Servings.Text = string.Format("# of Servings: {0}<br />", rec.ServingSize);
			
        }

        if (rec.User != null && !string.IsNullOrEmpty(rec.User.Name))
        {
            Creator.Text = string.Format("Recipe Created By: {0}<br />", rec.User.Name);
        }

        if (rec.Category != null && !string.IsNullOrEmpty(rec.Category.Name))
        {
            Category.Text = string.Format("Category: {0}<br />", rec.Category.Name);
        }

        foreach (var allergen in rec.Allergens)
        {
            if (allergen != null)
            {
                switch (allergen.Id)
                {
                    case 4:
                        //egg
                        ChkEgg.Checked = true;
                        break;
                    case 5:
                        //fish
                        ChkFish.Checked = true;
                        break;
                    case 8:
                        //gluten
                        ChkGluten.Checked = true;
                        break;
                    case 10:
                        //milk
                        ChkMilk.Checked = true;
                        break;
                    case 15:
                        //peanut
                        ChkPeanut.Checked = true;
                        break;
                    case 20:
                        //sesame
                        ChkSesame.Checked = true;
                        break;
                    case 21:
                        //shellfish
                        ChkShell.Checked = true;
                        break;
                    case 22:
                        //soy
                        ChkSoy.Checked = true;
                        break;
                    case 26:
                        //treenut
                        ChkTree.Checked = true;
                        break;
                    case 27:
                        //wheat
                        ChkWheat.Checked = true;
                        break;
                }
            }
        }

        
    }


    private string FillSubstitution(string p, List<IngredientDisplayModel> list)
    {
        bool addBreak = false;

        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(p)){
            sb.Append(p.Replace("\n","<br />"));
            addBreak = true;
        }

        List<double> ids = new List<double>();
        foreach (var t in list)
        {
            if (t.TipId.HasValue && !string.IsNullOrEmpty(t.TipDescription))
            {
                if (!ids.Contains(t.TipId.Value))
                {
                    if (addBreak)
                    {
                        sb.Append("<br /><br />");
                    }
                    else
                    {
                        addBreak = true;
                    }

                    sb.Append(t.TipDescription);
                    ids.Add(t.TipId.Value);
                }
            }
        }

        return sb.ToString();        
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        
        CommentView1.Visible = true;
        CommentView1.Post = BlogService.SelectPage(new Guid(Request.QueryString["id"])) as BlogEngine.Core.Page;


        int id = GetId();
        var da = new DataAccess();
        var rec = da.GetRecipe(id);

        if (rec == null)
            HttpContext.Current.Response.Redirect("/page/recipes-diet.aspx", true);


        var x = Page.Header.Controls.Cast<Control>().AsQueryable().Where(u => u.ID == "description" || u.ID == "keywords").ToList();
        foreach (var y in x)
        {
            Page.Header.Controls.Remove(y);
        }
      

        WriteRecipe(rec);
        RecipeRating.RecipeId = id;
        RecipeIdForImg.Value = id.ToString();

        //var pageNavItems = BlogService.FillPages().Where(p => p.IsDeleted == false && !p.NavOrder.Equals(string.Empty)).OrderBy(x => x.NavOrder).ToList();
        //foreach (var ogNAv2 in BlogEngine.Core.Page.Pages.Where(p => p.NavOrder.Equals(string.Empty)).OrderBy(x => x.NavOrder))
        //{
        //    if (Request.Url.ToString().ToLower().Contains(ogNAv2.Id.ToString().Replace("{", "").Replace("}", "")))
        //    {
        //        orphanPage = "home-page";
        //    }
        //}


        if (NavHelper.Instance.IsOphanPage(Request))
        {
            orphanPage = "home-page";
        }

        Page.Header.DataBind();
        pageHeading = Page.Header.Title;
        pageHeading = NavHelper.Instance.GetRootLevelTitle(Page.Header.Title);  
    }

    private int GetId()
    {
        try
        {
            int id = Convert.ToInt32(Request.QueryString["recid"]);
            return id;
        }
        catch (Exception ex)
        {
            return 0;
        }
    }

    private string GetIngredients(List<IngredientDisplayModel> list)
    {
        StringBuilder sb = new StringBuilder();
        List<int> items = new List<int>();
        foreach (var item in list.OrderBy(u => u.IngredientOrder))
        {
            if (!items.Contains(item.IngredientOrder))
            {
                items.Add(item.IngredientOrder);
                sb.AppendFormat("{0} {1} {2}{3}<br />", item.Quantity, item.Measurement, item.IngredientName, item.OptionalInfo == "Y" ? " (optional)" : string.Empty);
            }
        }

        return sb.ToString();
    }

    private string GetInstructions(string raw)
    {
        if (raw == null)
            return null;
        var rx = new Regex(@"\[[^\[]+\]\([^\(]+\)");
        var matches = rx.Matches(raw, 0);
        string t = rx.Replace(raw, LinkFliper);
        return t.Replace("\n", "<br />");
    }

    private string GetComments(string raw)
    {
        if (raw == null)
            return null;
        var rx = new Regex(@"\[[^\[]+\]\([^\(]+\)");
        var matches = rx.Matches(raw, 0);
        string t = rx.Replace(raw, LinkFliper);
        return t.Replace("\n", "<br />");
    }

    private string GetSubstitutions(string raw)
    {
        if (raw == null)
            return null;
        var rx = new Regex(@"\[[^\[]+\]\([^\(]+\)");
        var matches = rx.Matches(raw, 0);
        string t = rx.Replace(raw, LinkFliper);
        return t.Replace("\n", "<br />");
    }

    private Guid HasFirstLevelParent(Guid guid)
    {
        var firstLevelParetnGuid = new Guid();
        foreach (var ogNav in BlogEngine.Core.Page.Pages.Where(p => p.Id.Equals(guid)))
        {
            if (ogNav.HasParentPage)
            { firstLevelParetnGuid = ogNav.Parent; }
        }
        return firstLevelParetnGuid;
    }

    protected override void Render(HtmlTextWriter writer)
    {
        using (HtmlTextWriter htmlwriter = new HtmlTextWriter(new System.IO.StringWriter()))
        {
            base.Render(htmlwriter);
            writer.Write(htmlwriter.InnerWriter.ToString());
        }
    }

    protected string LinkFliper(Match match)
    {
        if (match.Success)
        {
            var v = match.Value.Replace("[", "").Replace(")", "");
            string[] parts = v.Split(new string[] { "](", @" """ }, StringSplitOptions.RemoveEmptyEntries);            
            var url = parts.Length > 1 ? parts[1] : "#";
            var text = parts.Length > 0 ? parts[0] : url;
            return string.Format(@"<a href=""{0}"" target=""_blank"">{1}</a>", url, text);
        }
        else
        {
            return match.Value;
        }
    }

    protected void RatingSubmit_Click(object sender, EventArgs e)
    {

    }

    protected void ImgSbmt_Click(object sender, EventArgs e)
    {
        if (FileUpload1.HasFile)
        {
            ImgUploadModel model = new ImgUploadModel();
            model.ImgName = FileUpload1.FileName;
            model.ImgData = FileUpload1.FileBytes;
            model.RecipeName = Title2.Text;
            model.RecipeId = Convert.ToInt32(RecipeIdForImg.Value);

            DataAccess da = new DataAccess();
            model = da.SubmitImgForRecipe(model);

            if (model.Success)
            {
                imgUp.Controls.Clear();
                imgUp.Controls.Add(new LiteralControl() { Text = @"<p id=""tu"">Thank you! Your image has been submitted for review.</p>" });
                imgUp.Controls.Add(new LiteralControl() { Text = "<script> $(document).ready(function () { window.location = '#img' });</script>" });
                imgUp.Focus();
            }
            else
            {
                uploadERrorMsg.Text = "There was an unexpected error while proccessing your request.  Please try again.";                
            }
        }
    }
}

public static class StringHelper
{
    public static string FormatUrls(this string input)
    {
        var x = Regex.Matches(input, @"\[[^\]]+\]\([^\)]+\)");
        return input;
        //[reddit!](http://reddit.com)
    }
}
