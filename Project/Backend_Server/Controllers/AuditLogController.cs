using Microsoft.AspNetCore.Mvc;
using Backend_Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
//Plan: Create a text crawler, read in userid, category, description, and timestamp. Then push that to the the database.
//Use an api. What would trigger it?
//Then create an admin page
//Scrap that, instead I am not creating a text crawler. the logs will just sort of exist. instead, whenever one is created, 
//I will create a new audit log thing in the database. This controller will just serve as a thingy. Where the dto will be.
//This will also have the api that the page will call to retrieve certain things from the database.