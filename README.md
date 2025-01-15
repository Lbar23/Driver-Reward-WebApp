# Project Documentation

## Project Overview

Git Gud Drivers (Get Good Drivers)

This project is a Web Application that incentivizes the improvement of on-road performance of truck drivers. It allows sponsors of many departments and standings within their own company to award drivers on good driving behaviors and rewards them accordingly. Drivers are also allowed to spend their point rewards on many catalog items to redeem. The application also aims to showcase Advanced DB Management and Web Accessibility.

## Tech Stack

### Frontend

- Core Languages: TypeScript (main page components, UI viewing), JavaScript (configuration files, default scripts, and static files)
- Frameworks/Libraries: React, Axios
- Build Tools: Vite (for fast development and optimized builds)

### Backend

- Languages/Frameworks: C#, ASP.NET Core, Entity Framework Core (Identity and Sql Server)
- (Main) Libraries: Newtonsoft.Json, Serilog, Swashbuckle (Swagger), OpenAPI, AWSSDK (Certain service packages)
- Databases: MySQL with support through ORM tools like EF Core

### Web Server

- NGINX: Used as a reverse proxy and static file server
- Tools: Certbot for SSL/TLS

### Integration

- Node.js: Utilized for asynchronous tasks, real-time processing, and database integration

## Key Features

- Advanced DB Management
  
- Web Accessibility

## Demo Data

The system is pre-loaded with the following data:

- An "open" driver application that can be rejected
- An "open" driver application that can be accepted
- Sponsoring Organizations (at least 4)
- Sponsor accounts (at least 2 per sponsor organization)
- Driver accounts (at least 2 per sponsor organization)
- Administrator accounts (at least 2)

## Running the Development Server

In order to run the development server, you'll first need to create an access key from the shared AWS account under your specific email profile.

1. Log In to your AWS account

2. Under your AWS account in the Top Right of your screen (<example@clemson.edu> @ cpsc4910), choose **Security Credentials**

3. Then scroll down until you find the **Access Key** tab and then click **"Create Access Key"**.

4. Choose **Local code**, agree to the terms, and click **Next**.

5. Click **Create Access Key** (While adding a descr tag is best practice, we don't have the current permissions for it).

You now have your access key for the shared account. (NOTE: DO NOT SHARE THIS IN THE REPO OR ANYWHERE ELSE; There is no "shared" access key, this is yours and yours alone).

It's recommended to save it as a CSV, but you can still retrieve its contents in case you forget.

Next;

Install the following VS Code extension:

**AWS Toolkit**

There you can either:

Click on AWS on the sidebar

Click on the Search Bar and Type **"> AWS New Connections"** button

After this, you should see sign in with AWS SSO or IAM credentials. Here, you choose the **IAM credentials**.

**Profile:** (can be anything; it's a log and reminder of who access the credentials; Recommended Name-team#-local-dev)

**Access Key:** (from the newly created access key)

**Secret Access Key:** (from the newly created access key)

And you're finished! Now you can safely and securely connect to the Database, run the development server locally, and test page components or APIs!

For the frontend_client folder, run:

```cmd
> npm install -que

> npm run build

> npm run dev
```

For the Backend_Server folder, run:

```cmd
> dotnet run
```

## DB ERD

<h1><img src="./Docs/db.png" alt="Database ERD" width="500"></h1>

## Website Wireframe

<h1><img src="./Docs/web.png" width="500"></h1>

## AWS Assets

All AWS assets used in the project are tagged with "TEAM 16" for identification purposes.
Examples can include:
team16
team-16
Team16
Team-16
etc...

## Team Members

**Damon Milford**

**Kristen Wright**

**Luis Barrera**

**Ryan Sands**

**Isaac Hine**

## unfortunately no longer works due to Amazon account being deleted!
