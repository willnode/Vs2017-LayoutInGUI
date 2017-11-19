# Vs2017-LayoutInGUI

This software generate arguments for downloading Visual Studio for offline Usage. Suitable for those who don't familiar with CLI (Command Line Interface).

![Screenshot](Info/Screenshot.png)

## Instruction

> Instruction below may inaccurate. Always adhere to [MSDN Manual][officialmanual] first.

In order:

1. [Download](./Releases) and Run the software
2. Select the edition
2. Set options (recommended/optional and language)
3. (Optional) Fetch the updated workload data.
4. Choose (and review) selected workloads and component.
5. Download the [stub installer][installer]. Run with the provided CLA (Command Line Argument) from this software.
6. CLI will appear. Wait until all component downloaded.
7. Open folder `C:\vs2017Layout`.
8. [Install certificates][certificates].
9. Run the setup (this time from layout folder) with provided CLA from this software. 
10. GUI Installation will appear, wait until installation complete.
11. (Optional) You can save the CLA to `BAT` file for future installation.

## How it works

This software fetch list of workload and component IDs from [GitHub mirror][workloadsgit] of their [List of Workloads](workloadsdoc) in their documentation. This is necessary because you will always get updated list of workloads, and I don't know other way to do it. 

If Microsoft changes or move their documentation layout or path, there will be chance that this application fail to parse, or even crash. If this happen, tell me.

This software always cache the downloaded package, so you don't have to spam the Github servers that kindly serve their docs to make this software possible.

## Disclaimer

This is a third-party software and does not represent any Microsoft Products.

License: [MIT](LICENSE)

> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

[installer]: https://docs.microsoft.com/en-us/visualstudio/install/install-vs-inconsistent-quality-network#step-1---download-the-visual-studio-bootstrapper
[officialmanual]: https://docs.microsoft.com/en-us/visualstudio/install/install-vs-inconsistent-quality-network
[workloadsdoc]: https://docs.microsoft.com/en-us/visualstudio/install/workload-and-component-ids
[workloadsgit]: https://github.com/MicrosoftDocs/visualstudio-docs/tree/master/docs/install
[certificates]: https://docs.microsoft.com/en-us/visualstudio/install/install-certificates-for-visual-studio-offline