# How to contribute

One of the easiest ways to contribute is to participate in discussions and discuss issues. You can also contribute by submitting pull requests with code changes.

## Code of Conduct
This project and everyone participating in it is governed by the [CHG-MERIDIAN Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to developers@chg-meridian.com.

## Bugs and feature requests?
For non-security related bugs please log a new issue in this GitHub repo.

## Reporting security issues and bugs
Security issues and bugs should be reported privately, via email, to the CHG-MERIDIAN security office  security@chg-meridian.com. You should receive a response very soon. If for some reason you do not, please follow up via email to ensure we received your original message.

## Filing issues
When filing issues, please use our bug and feature filing templates.
The best way to get your bug fixed is to be as detailed as you can be about the problem.
Providing a minimal project with steps to reproduce the problem is ideal.
Here are questions you can answer before you file a bug to make sure you're not missing any important information.

1. Did you include the snippet of broken code in the issue?
2. What are the *EXACT* steps to reproduce this problem?
3. What package versions are you using (you can see these in the `.csproj` file)?

GitHub supports [markdown](https://help.github.com/articles/github-flavored-markdown/), so when filing bugs make sure you check the formatting before clicking submit.


## Contributing code and content

### Identifying the scale

If you would like to contribute to one of our repositories, first identify the scale of what you would like to contribute. If it is small (grammar/spelling or a bug fix) feel free to start working on a fix. If you are submitting a feature or substantial code contribution, please discuss it with the team and ensure it follows the product roadmap. You might also read these two blogs posts on contributing code: [Open Source Contribution Etiquette](http://tirania.org/blog/archive/2010/Dec-31.html) by Miguel de Icaza and [Don't "Push" Your Pull Requests](https://www.igvita.com/2011/12/19/dont-push-your-pull-requests/) by Ilya Grigorik. 

### Obtaining the source code

If you are an outside contributor, please fork the repository you would like to contribute to your account. See the GitHub documentation for [forking a repo](https://help.github.com/articles/fork-a-repo/) if you have any questions about this. 

### Submitting a pull request

If you don't know what a pull request is read this article: https://help.github.com/articles/using-pull-requests. Make sure the respository can build and all tests pass. Familiarize yourself with the project workflow and our coding conventions. 

Pull requests should all be done to the **master** branch.

### Commit/Pull Request Format

```
Summary of the changes (Less than 80 chars)
 - Detail 1
 - Detail 2

Addresses #bugnumber (in this specific format)
```

### Tests

-  Tests need to be provided for every bug/feature that is completed.
-  Tests only need to be present for issues that need to be verified by QA (e.g. not tasks)
-  If there is a scenario that is far too hard to test there does not need to be a test for it.
  - "Too hard" is determined by the team as a whole.

### Feedback

Your pull request will now go through checks by the subject matter experts on our team. Please be patient when no feedback wil be given immediately. Update your pull request according to feedback until it is approved by one of the team members.
