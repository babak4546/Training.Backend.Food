using FoodApp.Api.Restaurants.DTOs;
using FoodApp.Core.Entities;
using FoodApp.Infrastructure.Data;
using FoodApp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

SecurityServices.AddServices(builder);
var app = builder.Build();
SecurityServices.UseServices(app);
app.MapPost("/createrequest",async (FoodAppDB db,ClaimsPrincipal user, RestaurantRequestDto restaurantRequest) =>
{
    var restaurant = new Restaurant
    {   Title=restaurantRequest.Title??"",
        Address=restaurantRequest.Address??"",
        ApprovedTime=null,
        ApproverUsername=null,
        CreationTime = DateTime.Now,
        IsActive=false, 
        IsApproved=false,
        OwnerUsername=user.Claims.FirstOrDefault(m=> m.Type=="Username")?.Value??""
    };
   await db.Restaurants.AddAsync(restaurant);
   await db.SaveChangesAsync();   
}).RequireAuthorization();
app.MapPost("/myrequestlist", async (FoodAppDB db,ClaimsPrincipal user) =>
{
    var username = user.Claims.FirstOrDefault(m => m.Type == "Username")?.Value;
    return Results.Ok( db
    .Restaurants
    .Where(m => m.IsApproved == false && m.OwnerUsername == username)
    .ToList());


}).RequireAuthorization();
app.MapPost("/approve", (FoodAppDB db, ClaimsPrincipal user,ApproveDto approve) =>
{
    if (user.Claims.FirstOrDefault(m => m.Type == "Type")?.Value != "SystemAdmin")
    {
        return Results.Unauthorized();
    }
    approve.Ids.ForEach(r =>
    {
        var restaurant= db.Restaurants.FirstOrDefault(m=>m.Id==r);
        restaurant.IsApproved = true;
        restaurant.ApproverUsername = user.Claims.FirstOrDefault(k => k.Type == "Username")?.Value;
        restaurant.ApprovedTime=DateTime.Now;
    });
    db.SaveChanges();
    return Results.Ok();
}).RequireAuthorization();
app.MapPost("/requestlist", async (FoodAppDB db,ClaimsPrincipal user) =>
{
    if (user.Claims.FirstOrDefault(m=>m.Type=="Type")?.Value!="SystemAdmin")
    {
        return Results.Unauthorized();
    }
    return Results.Ok(db.Restaurants
    .Include(m =>m.Owner)
    .Where(r => r.IsApproved == false)
    .Select(m=>new  RequestDto
    {
        Address = m.Address,
        Title = m.Title,
        Id = m.Id,  
        OwnerUsername = m.OwnerUsername,
        Email=m.Owner.Email,
        Fullname=m.Owner.Fullname
    })
    .ToList());
        
}).RequireAuthorization();
app.MapGet("/requestcount", async (FoodAppDB db) =>
{
return Results.Ok(
    new
    {
      Count=await db.Restaurants.CountAsync(c=>c.IsApproved==false)
    });
});
 app.Run();

