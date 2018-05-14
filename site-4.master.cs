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
    protected void Page_Load(object sender, EventArgs e)
    {
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

        if (!IsPostBack)
        {
            Repeater1.DataSource = new object[] { new Object(), new Object(), new Object() };
            Repeater1.DataBind();

            DataAccess da = new DataAccess();
            RecipeCategory.DataSource = da.GetCategoryRecipeList();
            RecipeCategory.DataTextField = "Name";
            RecipeCategory.DataValueField = "Id";
            RecipeCategory.AppendDataBoundItems = true;
            RecipeCategory.DataBind();
        }
        //if (Security.IsAuthenticated)
        //{
        //    aLogin.InnerText = Resources.labels.logoff;
        //    aLogin.HRef = Utils.RelativeWebRoot + "Account/login.aspx?logoff";
        //}
        //else
        //{
        //    aLogin.HRef = Utils.RelativeWebRoot + "Account/login.aspx";
        //    aLogin.InnerText = Resources.labels.login;
        //}
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

    protected void SubmitLink_Click(object sender, EventArgs e)
    {
        RecipeCreateModel model = new RecipeCreateModel();
        model.UserEmail = UserEmail.Text;
        model.UserName = UserName.Text;
        model.RecipeName = RecipeName.Text;
        model.Description = RecipeDescription.Text;
        model.ServingSize = ServingSize.Text;
        model.Directions = Directions.Text;
        model.SubsitutionNote = Substitutions.Text;
        model.Notes = Notes.Text;

        if (FileUpload1.HasFile)
        {
            model.ImgName = FileUpload1.FileName;
            model.ImgData = FileUpload1.FileBytes;
        }

        model.Ingredients = GetIngredients();
        try
        {
            model.CategoryId = Convert.ToInt32(RecipeCategory.SelectedValue);
        }
        catch (Exception ex)
        {}

        model.AllergenList = new List<int>();
                
        if (ChkEgg.Checked)
            model.AllergenList.Add(4);
        if (ChkFish.Checked)
            model.AllergenList.Add(5);
        if (ChkGluten.Checked)
            model.AllergenList.Add(8);
        if (ChkMilk.Checked)
            model.AllergenList.Add(10);
        if (ChkPeanut.Checked)
            model.AllergenList.Add(15);
        if (ChkSesame.Checked)
            model.AllergenList.Add(20);
        if (ChkShell.Checked)
            model.AllergenList.Add(21);
        if (ChkSoy.Checked)
            model.AllergenList.Add(22);
        if (ChkTree.Checked)
            model.AllergenList.Add(26);
        if (ChkWheat.Checked)
            model.AllergenList.Add(27);


        DataAccess da = new DataAccess();
        model = da.CreateRecipe(model);

        if (model.Success)
        {
            Response.Redirect("/page/submit-new-recipe-thank-you.aspx");
        }
        else
        {
            //TODO: error messageing
        }
    }

    private List<IngredientCreateModel> GetIngredients()
    {
        List<IngredientCreateModel> ingredients = new List<IngredientCreateModel>();

        int counter = 0;
        
        //IngredientCreateModel model = PullIngredientModel(counter);
        foreach(string key in Request.Form.AllKeys)
        {
            var val = Request.Form[key];
            
            if(key.StartsWith("Ing"))
            {
                int index = -1; 
                Match m1 = Regex.Match(key, "[0-9]+");
                if(m1.Success)
                {
                    try
                    {
                        index = Convert.ToInt32(m1.Value);
                    }
                    catch (Exception ex)
                    { }
                }

                if (index > -1)
                {
                    if (!ingredients.Any(u => u.Index == index))
                    {
                        ingredients.Add(new IngredientCreateModel() { Index = index });
                    }
                    var m = ingredients.FirstOrDefault(u => u.Index == index);

                    if (key.Contains("Measurement"))
                        m.Measurement = val;
                    else if (key.Contains("Ingredient"))
                        m.Ingredient = val;
                    else if (key.Contains("Optional"))
                        m.Optional = true;
                    else if (key.Contains("Qty"))
                        m.Qty = val;
                }
                else
                {
                    //cant do anytyhing with it
                }
            }
        }

        List<IngredientCreateModel> final = new List<IngredientCreateModel>();
        
        int counter2 = 0;
        foreach (var ing in ingredients.OrderBy(u => u.Index))
        {
            if (ing.IsValid)
            {
                ing.Index = counter2++;
                final.Add(ing);
            }
        }

        return final;
    }

}
