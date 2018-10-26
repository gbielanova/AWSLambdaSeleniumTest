Setup
=====

Setup AWS Lambda User permissions
--------------------
User that is used for deployment from .NET CLI command (for some reason in our test its the apitestthumbnailsnspush user, maybe because its the first one in the list) should have following permissions:
{
    "Version": "2012-10-17",
    "Statement": [
    {
        "Sid": "Stmt1482712489000",
        "Effect": "Allow",
        "Action": [
        "iam:CreateRole",
        "iam:PutRolePolicy",
        "lambda:CreateFunction",
        "lambda:InvokeAsync",
        "lambda:InvokeFunction",
        "iam:PassRole",
        "lambda:UpdateAlias",
        "lambda:CreateAlias",
        "lambda:GetFunctionConfiguration",
        "lambda:AddPermission",
        "lambda:UpdateFunctionCode"
    ],
    "Resource": [
        "*"
    ]}]
}

Create AWS Lambda Role
---------------------------
Go to IAM section of AWS and to permissions and then create a new role and select following predefined permissions on the next screen: 
AWSLambdaBasicExecutionRole
Then add following inline policy for be able to work with function:
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Action": [
                "lambda:GetFunction",
                "lambda:GetFunctionConfiguration"
            ],
            "Resource": [
                "*"
            ],
            "Effect": "Allow"
        }
    ]
}

Creating and running simple Selenium AWS function project with .NET CLI
* Install VS Code
* Install latest .NET Core SDK
* In VS code use terminal to install AWS tools for .NET CLI with the command: dotnet new -i Amazon.Lambda.Templates
* Create new project with: dotnet new lambda.EmptyFunction, use profile and region to specify user and region of the function
* Add selenium packages with: dotnet add package selenium.webdriver and dotnet add package Selenium.WebDriver.ChromeDriver
* Modify csproj file to include console headless chrome and chromedriver executables in the build folder by adding following section:
&#060;ItemGroup>
    &#060;None Update="lib/**" Link="%(Filename)%(Extension)">
        &#060;CopyToOutputDirectory>Always&#060;/CopyToOutputDirectory>
    &#060;None>
&#060;/ ItemGroup >
In the example "lib" is the folder with the files that is located in the folder with the project.

* In the code setup ChromeDriver options. ChromeDriver binary is searched in the same folder as the function's dll, but for the chrome we need to specify Binary location property of ChromeOptions. Base folder for our function is always "/var/task", so the chrome binary path will be "/var/task/chrome" in the described case.
* Perform any operation with the ChromeBrowser in the code and return some result. Use static LambdaLogger.Log for trace (alternatively just use Console.WriteLine).
* Use dotnet build and dotnet test commands to build project and to run test project (remember to use full path to the test project in dotnet test).
* Use following command to package the project into a zip you can manually upload for AWS: dotnet lambda package
* Better use following command for packaging and deploying the function directly to AWS Lambda (should be logged in with browser I suppose): dotnet lambda deploy-function -fn GetSiteInfo -frole arn:aws:iam::598592458233:role/SeleniumTestRole
Here GetSiteInfo is the desired function name and frole partameter is the name of the role we created earlier.
* You can invoke function with a test in the AWS console or
* You can call function with the command: dotnet lambda invoke-function GetSiteInfo --payload "http://www.google.com" where payload is the parameter of the function (string input in the default test project).
