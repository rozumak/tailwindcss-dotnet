# Tailwind CSS for ASP.NET Core

A [dotnet tool](https://www.nuget.org/packages/tailwindcss-dotnet) that simplifies the installation and usage of [Tailwind CSS](https://tailwindcss.com) in ASP.NET Core projects by utilizing the stand-alone [Tailwind CSS CLI](https://github.com/tailwindlabs/tailwindcss/tree/master/standalone-cli).

## Getting Started

### Step 1: Create your project

Start by creating a new ASP.NET Core project if you don’t have one set up already. You can use any web app template of your choice.

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

And start the app hot-reload dev server:

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

## Developing with Tailwind CSS

### Overview

[Tailwind CSS](https://tailwindcss.com) is a CSS framework that uses a utility-first approach to styling elements. It allows you to apply pre-existing classes directly in your templates. The `tailwindcss-dotnet` tool wraps the standalone executable of the Tailwind CSS v3 framework, which is platform-specific and not bundled within the tool. When you run the tool for the first time, it downloads and saves the required executables automatically.

It is supports the same platforms as the native Tailwind CLI executable, including Windows, macOS, and Linux.

Starting from version 3, Tailwind CSS uses the Just-In-Time (JIT) technique to generate styles. It scans specified template files for class names and generates corresponding styles only for the names it finds. This means that you cannot generate class names programmatically. If you need styles for names that don't exist in your content files, you can use the [safelist option](https://tailwindcss.com/docs/content-configuration#safelisting-classes). However, it is not recommended to do so.

### Installation

To use Tailwind CSS in your dotnet project, follow these steps:

1. Install the tool by running the command `dotnet tool install --global tailwindcss-dotnet`.
2. Run the command `tailwind install`.

To run tool commands for a specific project from any location, you can use the `--project` option followed by the relative file location of the project. This option applies to all tool commands. For example, `tailwind build --project MyProject\MyProject.csproj` will generate the output CSS file for the specified project.

When you install the tool, it creates default `styles\app.tailwind.css` and `tailwind.config.js` input files, which are configured to be used within your ASP.NET Core project.

You can specify the imports you want to use, set up your `@apply` rules and custom CSS, customize the Tailwind build, just like in a traditional Node installation. Note that only first-party plugins are supported.

### Building for Production

Run the `tailwind build` command to generate the output CSS file at `wwwroot\css\app.css`. This file should be included in your app HTML layout (the installer configures this automatically).

It is recommended to not include the output CSS file in source control. Instead, run this command in your CI environment.

### Building for Development

To automatically reflect changes in the generated CSS output while developing your application, run Tailwind in "watch" mode by executing the command `tailwind watch` in a separate process.

If file system events are unreliable or not supported, pass the `--poll` argument to use polling instead: `tailwind watch --poll`.

### Configuring CSS minification

By default, minified assets will be generated. If you want to change this behavior, pass a `--debug` argument to the command, for example, `tailwind build --debug` or `tailwind watch --debug`.

### Customizing Tailwind inputs or outputs

The default paths are opinionated. If you need to use custom file paths or make other changes not covered by default behavior, you can access the platform-specific executable directly by running the command `tailwind exec` and passing any necessary command line arguments to the Tailwind CLI.

### Override Tailwind CLI version

If you need to use a specific version of Tailwind CLI for your project, you can do so by specifying the version using the `--tailwindcss` option. If nothing is specified then the latest release of the tailwindcss executable is pulled.

For example, to use Tailwind CLI of version 3.2.1, you can use the following command:
`tailwind build --tailwindcss v3.2.1`
