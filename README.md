# Tailwind CSS for ASP.NET Core

A [dotnet tool](https://www.nuget.org/packages/tailwindcss-dotnet) that simplifies the installation and usage of [Tailwind CSS](https://tailwindcss.com) in ASP.NET Core projects by utilizing the stand-alone [Tailwind CSS CLI](https://github.com/tailwindlabs/tailwindcss/tree/master/standalone-cli).

## Getting Started

### Step 1: Create your project

Start by creating a new ASP.NET Core project if you don’t have one set up already.

```
dotnet new blazorserver -o MyProject
cd MyProject
```

### Step 2: Install Tailwind CSS

Install [dotnet tool](https://www.nuget.org/packages/tailwindcss-dotnet) globally, and then run the `tailwind install` command to generate the `tailwind.config.js` and `styles\app.tailwind.css` files:

```
dotnet tool install --global tailwindcss-dotnet
tailwind install
```

### Step 3: Start your build process

Run your tailwind build process with:

```
tailwind watch
```

And start app hot-reload dev server:

```
dotnet run watch
```

### Step 4: Start using Tailwind in your project

Start using Tailwind’s utility classes to style your content.

```csrazor
<h1 class="text-3xl font-bold underline">
  Hello world!
</h1>
```
