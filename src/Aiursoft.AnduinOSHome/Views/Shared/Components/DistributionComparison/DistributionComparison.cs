using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.AnduinOSHome.Views.Shared.Components.DistributionComparison;

public class DistributionComparison : ViewComponent
{
    private const string AnduinPackages = "https://github.com/AiursoftWeb/AnduinOS-Packages";
    private const string AnduinIso = "https://github.com/AiursoftWeb/AnduinOS-2";
    private const string InstallerDesign = AnduinPackages + "/blob/master/anduinos-installer-beta/DESIGN.md";
    private const string BtrfsDesign = AnduinPackages + "/blob/master/anduinos-installer-beta/BTRFS-DESIGN.md";
    private const string SwapDesign = AnduinPackages + "/blob/master/anduinos-swapcontrol-gtk/ARCHITECTURE.md";
    private const string SnapshotDesign = AnduinPackages + "/tree/master/anduinos-btrfs-snapshots-manager";
    private const string ZorinDetails = "https://zorin.com/os/details/";
    private const string ZorinNvidia = "https://help.zorin.com/docs/hardware/activate-nvidia-drivers/";
    private const string ZorinWindowsApps = "https://help.zorin.com/docs/apps-games/windows-app-support/";
    private const string MintRelease = "https://blog.linuxmint.com/?p=4981";
    private const string MintHwe = "https://blog.linuxmint.com/?p=5050";
    private const string MintWayland = "https://blog.linuxmint.com/?p=5046";
    private const string MintTimeshift = "https://linuxmint-installation-guide.readthedocs.io/en/latest/timeshift.html";
    private const string UbuntuRelease = "https://documentation.ubuntu.com/release-notes/26.04/summary-for-lts-users/";
    private const string UbuntuDesktop = "https://ubuntu.com/download/desktop";
    private const string UbuntuSecureBoot = "https://documentation.ubuntu.com/security/docs/security-features/platform-protections/secure-boot/";
    private const string GnomeRelease = "https://release.gnome.org/50/";

    public IViewComponentResult Invoke()
    {
        return View(new DistributionComparisonViewModel
        {
            Items = BuildItems()
        });
    }

    private static IReadOnlyList<ComparisonItem> BuildItems()
    {
        return
        [
            new ComparisonItem(
                "platform", "crown", "Platform generation", "Ubuntu foundation and kernel", true,
                Cell(ComparisonLevel.FirstClass, "Ubuntu 26.04 · Linux 7.0 HWE",
                    "AnduinOS 2 targets Ubuntu 26.04 Resolute. Its core package owns the Linux 7.0 HWE package contract, so the kernel is retained through normal installs and upgrades instead of being an ISO-only customization.",
                    Source("AnduinOS package architecture", AnduinPackages)),
                Cell(ComparisonLevel.DefaultProvided, "Ubuntu 24.04 · Linux 6.17",
                    "Zorin OS 18.1 uses the Ubuntu 24.04 LTS foundation and Linux 6.17. Its desktop and userspace therefore remain on the Ubuntu 24.04 generation rather than the Ubuntu 26.04 and GNOME 50 generation used by AnduinOS 2.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Ubuntu 24.04 · 6.14 / 7.0 HWE",
                    "Linux Mint 22.3 was released with Linux 6.14. Mint now also publishes fully QA-tested HWE images with Linux 7.0 for newer hardware, so the selected image matters.",
                    Source("Linux Mint 22.3 release", MintRelease), Source("Linux 7.0 HWE images", MintHwe)),
                Cell(ComparisonLevel.FirstClass, "Ubuntu 26.04 · Linux 7.0",
                    "Ubuntu 26.04 LTS is the upstream reference for this generation and ships the Linux 7.0 kernel series with five years of standard LTS maintenance.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "wayland", "monitor-up", "Wayland desktop", "Primary display architecture", true,
                Cell(ComparisonLevel.FirstClass, "Wayland-enforced GNOME",
                    "AnduinOS ships a GNOME Wayland session as the desktop contract. XWayland remains available for legacy applications, while the desktop itself does not fall back to an Xorg session.",
                    Source("AnduinOS desktop packages", AnduinPackages)),
                Cell(ComparisonLevel.DefaultProvided, "Wayland with Xorg fallback",
                    "Zorin OS 18 defaults to Wayland while retaining selectable Xorg sessions. The fallback is useful for compatibility, particularly when troubleshooting graphics drivers.",
                    Source("Zorin NVIDIA and session guidance", ZorinNvidia)),
                Cell(ComparisonLevel.Experimental, "X11 default · Wayland experimental",
                    "Cinnamon's Wayland session remains experimental in Linux Mint 22.3. Mint has announced that both X11 and Wayland will become fully supported in the next Cinnamon release.",
                    Source("Linux Mint Wayland status", MintWayland)),
                Cell(ComparisonLevel.FirstClass, "Wayland-only GNOME",
                    "Ubuntu 26.04 runs the GNOME desktop session only on Wayland. XWayland is retained for applications written for X.org.",
                    Source("Ubuntu Wayland session", UbuntuRelease))),

            new ComparisonItem(
                "architectures", "cpu", "Desktop architectures", "Installable AMD64 and ARM64 products", true,
                Cell(ComparisonLevel.FirstClass, "AMD64 + ARM64",
                    "Both architectures are product targets. The ISO builder, native installer, UEFI layout, signed boot payloads and acceptance matrix model AMD64 and ARM64 explicitly.",
                    Source("AnduinOS ISO source", AnduinIso), Source("Installer design", InstallerDesign)),
                Cell(ComparisonLevel.Unavailable, "Official x86-64 desktop",
                    "Zorin's official desktop images target 64-bit Intel and AMD processors. No official ARM64 desktop product is listed for Zorin OS 18.1.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.Unavailable, "Official x86-64 images",
                    "Linux Mint 22.3 publishes its Cinnamon, MATE and Xfce desktop editions as 64-bit x86 images. No equivalent official ARM64 Mint desktop image is offered.",
                    Source("Linux Mint 22.3 release", MintRelease)),
                Cell(ComparisonLevel.FirstClass, "AMD64 + ARM64",
                    "Canonical publishes official Ubuntu 26.04 desktop downloads for both Intel or AMD 64-bit systems and ARM 64-bit systems.",
                    Source("Ubuntu Desktop downloads", UbuntuDesktop))),

            new ComparisonItem(
                "secure-boot", "shield-check", "Secure Boot lifecycle", "Boot trust, MOK and third-party modules", true,
                Cell(ComparisonLevel.FirstClass, "Installer + MOK + DKMS health",
                    "Secure Boot is modeled during installation on AMD64 and ARM64. A shared GTK4 toolkit exposes MOK enrollment and DKMS signing health to the installer, OOBE and Driver Center.",
                    Source("Installer Secure Boot design", InstallerDesign), Source("AnduinOS packages", AnduinPackages)),
                Cell(ComparisonLevel.Supported, "Ubuntu trust chain",
                    "Zorin supports Secure Boot through its Ubuntu foundation and documents firmware fallbacks for problem hardware. NVIDIA guidance may still recommend disabling Secure Boot or using Xorg when drivers fail.",
                    Source("Zorin Secure Boot guidance", ZorinNvidia)),
                Cell(ComparisonLevel.Supported, "Ubuntu signed foundation",
                    "Linux Mint inherits Ubuntu's signed shim, GRUB and kernel foundation. Mint documents workarounds for systems that report Secure Boot violations.",
                    Source("Ubuntu Secure Boot architecture", UbuntuSecureBoot)),
                Cell(ComparisonLevel.FirstClass, "Canonical-signed chain",
                    "Ubuntu provides Microsoft-signed shim, Canonical-signed GRUB, kernels and modules on AMD64 and ARM64, together with mature MOK and DKMS signing workflows.",
                    Source("Ubuntu Secure Boot architecture", UbuntuSecureBoot))),

            new ComparisonItem(
                "btrfs", "database", "Default storage model", "Filesystem defaults and system topology", true,
                Cell(ComparisonLevel.FirstClass, "Btrfs default · ext4 optional",
                    "The native installer selects Btrfs by default and creates named subvolumes that define snapshot, home, log, container, virtual-machine and recovery boundaries. ext4 remains available as a classic alternative.",
                    Source("Btrfs system design", BtrfsDesign), Source("Installer design", InstallerDesign)),
                Cell(ComparisonLevel.NotDocumented, "ext4-centered default",
                    "Zorin follows an ext4-centered Ubuntu 24.04 installation path. Its reviewed official product information does not document a default Zorin-owned Btrfs topology and recovery ABI.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.Supported, "ext4 default · Btrfs selectable",
                    "Mint installs to ext4 by default. Btrfs can be selected and Timeshift can use Btrfs snapshots, but Mint does not define the same default subvolume ABI as AnduinOS.",
                    Source("Linux Mint system snapshots", MintTimeshift)),
                Cell(ComparisonLevel.Supported, "ext4 default · other options",
                    "Ubuntu's guided desktop installation remains ext4-centered. Other layouts can be configured, but Ubuntu does not ship the AnduinOS Btrfs desktop ABI as its default.",
                    Source("Ubuntu Desktop installation", UbuntuDesktop))),

            new ComparisonItem(
                "recovery", "history", "System recovery model", "Snapshots, update protection and rollback", true,
                Cell(ComparisonLevel.FirstClass, "APT snapshots + bootable rollback",
                    "System snapshots are connected to APT transactions, retention policy, Dracut, GRUB, deployment metadata and boot confirmation. Recovery is an operating-system protocol rather than a standalone backup utility.",
                    Source("Disk Snapshots Manager", SnapshotDesign), Source("Btrfs system design", BtrfsDesign)),
                Cell(ComparisonLevel.NotDocumented, "Deployment rollback not documented",
                    "The reviewed public Zorin product information does not document a default Zorin-owned stack connecting filesystem deployments, package transactions, boot entries and confirmation services.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Timeshift system snapshots",
                    "Timeshift provides Mint system snapshots with scheduling and retention. Its restoration model is separate from the bootable-deployment and package-transaction protocol used by AnduinOS.",
                    Source("Linux Mint system snapshots", MintTimeshift)),
                Cell(ComparisonLevel.NotDocumented, "Deployment rollback not documented",
                    "Ubuntu provides recovery mechanisms and third-party options. Its reviewed default desktop information does not document an integrated protocol connecting Btrfs deployments, APT and boot confirmation.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "memory", "memory-stick", "Memory pressure strategy", "Compressed RAM, disk Swap and health", true,
                Cell(ComparisonLevel.FirstClass, "Zram + disk Swap + health UI",
                    "AnduinOS layers 50%-of-RAM LZ4 Zram above a dynamically sized disk Swap partition. The native GTK4 tool manages Zram, optional Zswap, stress tests and hibernation health.",
                    Source("Swap Control architecture", SwapDesign), Source("Installer design", InstallerDesign)),
                Cell(ComparisonLevel.Supported, "Ubuntu-derived policy",
                    "Zorin inherits the Ubuntu 24.04 memory and Swap foundation. Its reviewed public product information does not document a Zorin-owned virtual-memory control plane of this scope.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.NotDocumented, "Standard memory and Swap controls",
                    "Linux Mint provides standard Linux memory and Swap capabilities. Its reviewed release information does not document a default integrated Zram, Zswap, stress-test and hibernation-health interface.",
                    Source("Linux Mint 22.3 release", MintRelease)),
                Cell(ComparisonLevel.DefaultProvided, "General-purpose policy",
                    "Ubuntu supplies the underlying kernel memory features and a general-purpose desktop policy, without the same default AnduinOS Zram, partition and health-dashboard contract.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "native-tools", "panels-top-left", "First-party system tools", "Toolkit and graphical control surfaces", true,
                Cell(ComparisonLevel.FirstClass, "GTK4/libadwaita throughout",
                    "Installer, Control Panel, Appearance, OOBE, Driver Center, Secure Boot Toolkit, Swap Control, Snapshot Manager and other AnduinOS tools share GTK4, libadwaita and adaptive conventions.",
                    Source("AnduinOS component catalog", AnduinPackages)),
                Cell(ComparisonLevel.DefaultProvided, "Mixed first-party estate",
                    "Zorin combines GNOME applications, extensions and Zorin-specific tools from the Ubuntu 24.04 generation. Its public product information does not describe a whole first-party control-plane migration to GTK4 and libadwaita.",
                    Source("Zorin OS 18.1 details", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Cinnamon and XApp tools",
                    "Mint deliberately maintains its Cinnamon and XApp design system. Major administration tools remain primarily GTK3/XApp while GTK4 applications are supported.",
                    Source("Linux Mint 22.3 release", MintRelease)),
                Cell(ComparisonLevel.DefaultProvided, "Modern GNOME applications",
                    "Ubuntu benefits from modern GTK4 and libadwaita GNOME applications, while its installer and distribution-specific surfaces use a broader mix of technologies.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease), Source("GNOME 50 release notes", GnomeRelease))),

            new ComparisonItem(
                "modern-graphics", "sparkles", "Display pipeline", "HDR, VRR and fractional scaling", false,
                Cell(ComparisonLevel.FirstClass, "GNOME 50 graphics pipeline",
                    "AnduinOS inherits GNOME 50's HDR screen sharing, Wayland color-management v2, improved VRR, optimized fractional scaling and low-latency cursor path.",
                    Source("GNOME 50 display improvements", GnomeRelease)),
                Cell(ComparisonLevel.Supported, "Ubuntu 24.04 GNOME generation",
                    "Zorin's Ubuntu 24.04-era GNOME foundation supports useful Wayland display features, but predates the GNOME 50 HDR, VRR and color-management generation.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.Experimental, "Wayland graphics still maturing",
                    "Cinnamon 6.6 remains centered on its mature X11 path in Mint 22.3. The next release is where Mint plans to graduate Wayland support from experimental status.",
                    Source("Linux Mint Wayland status", MintWayland)),
                Cell(ComparisonLevel.FirstClass, "GNOME 50 graphics pipeline",
                    "Ubuntu 26.04 ships GNOME 50 and its current Wayland display improvements, including optimized fractional scaling.",
                    Source("Ubuntu Desktop", UbuntuDesktop), Source("GNOME 50 release notes", GnomeRelease))),

            new ComparisonItem(
                "nvidia", "cpu", "NVIDIA on Wayland", "Drivers, frame timing and recovery paths", false,
                Cell(ComparisonLevel.FirstClass, "Wayland + Driver Center",
                    "GNOME 50 frame-timing improvements, Ubuntu 26.04 drivers and the AnduinOS Driver Center are connected to Secure Boot and DKMS trust health.",
                    Source("AnduinOS packages", AnduinPackages), Source("GNOME 50 display improvements", GnomeRelease)),
                Cell(ComparisonLevel.Supported, "Modern drivers · Xorg fallback",
                    "Zorin offers a modern-NVIDIA Live boot option and proprietary-driver workflow. Its own troubleshooting guide retains Xorg and Secure Boot fallbacks.",
                    Source("Zorin NVIDIA guidance", ZorinNvidia)),
                Cell(ComparisonLevel.Experimental, "Wayland improvements in progress",
                    "Mint announced accelerated NVIDIA Wayland improvements for the next Cinnamon release. In 22.3, the Wayland session itself remains experimental.",
                    Source("Linux Mint Wayland status", MintWayland)),
                Cell(ComparisonLevel.FirstClass, "Fully supported on Wayland",
                    "Ubuntu 26.04 states that machines using NVIDIA graphics are fully supported on its Wayland desktop session.",
                    Source("Ubuntu Wayland session", UbuntuRelease))),

            new ComparisonItem(
                "dracut", "workflow", "Early-boot architecture", "Initramfs implementation and migration policy", false,
                Cell(ComparisonLevel.FirstClass, "Single verified Dracut stack",
                    "AnduinOS core depends on Dracut and conflicts with initramfs-tools, Casper and finalrd. Generated images are verified and guarded so competing generators cannot silently coexist.",
                    Source("AnduinOS package architecture", AnduinPackages), Source("AnduinOS ISO source", AnduinIso)),
                Cell(ComparisonLevel.DefaultProvided, "Inherited initramfs-tools + Casper",
                    "Zorin OS 18.1 uses the Ubuntu 24.04 initramfs-tools and Casper generation. This is its default inherited early-boot design rather than a single pure-Dracut product contract.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Inherited Ubuntu 24.04 stack",
                    "Linux Mint 22.3 uses the Ubuntu 24.04-era early-boot and Live generation. The reviewed public material did not identify a Mint-specific single-generator Dracut contract.",
                    Source("Linux Mint 22.3 release", MintRelease)),
                Cell(ComparisonLevel.DefaultProvided, "Dracut default · compatibility retained",
                    "Ubuntu now uses Dracut by default, but initramfs-tools remains supported. Ubuntu 26.04 is therefore a transition-compatible implementation rather than an exclusive single-generator policy.",
                    Source("Ubuntu Dracut transition", UbuntuRelease))),

            new ComparisonItem(
                "live-layers", "layers-3", "Live system layers", "Temporary and persistent USB overlays", false,
                Cell(ComparisonLevel.FirstClass, "Dracut Live Layers",
                    "The ISO uses dmsquash-live plus AnduinOS Live Layers for temporary Try mode, persistent USB overlays, media preservation, installer-source exposure and expanded-USB repair.",
                    Source("AnduinOS ISO architecture", AnduinIso), Source("AnduinOS packages", AnduinPackages)),
                Cell(ComparisonLevel.Supported, "Ubuntu-derived Live persistence",
                    "Zorin provides an Ubuntu-derived Live environment. Its public product information does not document a distribution-owned Dracut Live Layers and installer-source contract.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.Supported, "Mint Live session",
                    "Mint offers a Live desktop and persistence workflows. Its reviewed public information does not document a Mint-owned Dracut overlay protocol.",
                    Source("Linux Mint 22.3 release", MintRelease)),
                Cell(ComparisonLevel.Supported, "Ubuntu Desktop Live media",
                    "Ubuntu provides robust official Live desktop media, using its own installer and media lifecycle rather than the AnduinOS Live Layers contract.",
                    Source("Ubuntu Desktop downloads", UbuntuDesktop))),

            new ComparisonItem(
                "installer", "hard-drive-download", "Installer architecture", "UI, privilege boundary and execution model", false,
                Cell(ComparisonLevel.FirstClass, "Native GTK4 · typed install plan",
                    "An unprivileged GTK4 planner produces a canonical typed plan. A fixed privileged executor revalidates it and constructs commands itself; the UI cannot inject arbitrary shell commands or paths.",
                    Source("Native installer design", InstallerDesign)),
                Cell(ComparisonLevel.DefaultProvided, "Established installer path",
                    "Zorin offers an Ubuntu-derived graphical installation experience. Its reviewed public product information does not document a typed-plan and fixed semantic executor contract.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Mint graphical installer",
                    "Mint provides its established graphical installer. Its reviewed public design does not document the native GTK4 planner and fixed semantic executor contract used by AnduinOS.",
                    Source("Linux Mint 22.3 release announcement", MintRelease)),
                Cell(ComparisonLevel.DefaultProvided, "Modern Flutter frontend",
                    "Ubuntu's current desktop installer has a modern frontend/backend architecture and broad platform engineering, but it is a different design from the native GTK4 AnduinOS plan contract.",
                    Source("Ubuntu Desktop installation", UbuntuDesktop))),

            new ComparisonItem(
                "package-engineering", "package-check", "System composition", "Package source and product composition", false,
                Cell(ComparisonLevel.FirstClass, "APKG + .aosproj projects",
                    "AnduinOS package projects declare suites, architectures, dependencies, conflicts, assets, services, AppStream metadata, lifecycle scripts and build gates together. Layered metapackages compose the final product.",
                    Source("AnduinOS package architecture", AnduinPackages)),
                Cell(ComparisonLevel.DefaultProvided, "Debian packages + Zorin composition",
                    "Zorin uses mature Debian and Ubuntu packaging together with its own repositories and product package selection. It follows a different source and composition model.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Debian packages + Mint tooling",
                    "Mint uses established Debian packaging, metapackages and Mint project tooling. It does not use the unified AnduinOS .aosproj format.",
                    Source("Linux Mint 22.3 release", MintRelease)),
                Cell(ComparisonLevel.FirstClass, "Seeds, metapackages and archive QA",
                    "Ubuntu has seed, metapackage, archive and automated-testing infrastructure at much larger scale. Its implementation model differs from APKG and .aosproj projects.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "apt-snapshots", "package-plus", "Package transaction safety", "Recovery points around APT changes", false,
                Cell(ComparisonLevel.FirstClass, "Automatic pre/post snapshots",
                    "APT hooks create structured recovery points before and after package changes. Snapshot metadata records package state and feeds the same retention and rollback protocol.",
                    Source("Disk Snapshots Manager", SnapshotDesign)),
                Cell(ComparisonLevel.NotDocumented, "No default APT snapshot hook documented",
                    "The reviewed public Zorin product information does not document a default policy creating symmetric recovery points around each APT transaction.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.Supported, "Timeshift advised before upgrades",
                    "Mint explicitly recommends creating a Timeshift snapshot before a release upgrade. This is a manual upgrade safeguard rather than an automatic symmetric APT transaction contract.",
                    Source("Linux Mint system snapshots", MintTimeshift)),
                Cell(ComparisonLevel.NotDocumented, "No default APT snapshot hook documented",
                    "Ubuntu supports many backup and snapshot technologies. Its reviewed default desktop information does not document automatic recovery points around every APT transaction.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "personal-history", "folder-clock", "Personal file history", "Separate recovery for user data", false,
                Cell(ComparisonLevel.FirstClass, "Personal Files snapshots",
                    "AnduinOS separates system recovery from Personal Files history. The Snapshot Manager and Files integration can browse and restore earlier versions without conflating them with OS rollback.",
                    Source("Disk Snapshots Manager", SnapshotDesign), Source("Btrfs system design", BtrfsDesign)),
                Cell(ComparisonLevel.NotDocumented, "No integrated personal history documented",
                    "The reviewed public Zorin product information does not document a default Zorin-owned personal-file snapshot history integrated with its filesystem and file manager.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.Unavailable, "Timeshift covers system state",
                    "Timeshift intentionally protects system state and is not a personal backup tool. Personal files require a separate backup approach.",
                    Source("Linux Mint system snapshots", MintTimeshift)),
                Cell(ComparisonLevel.NotDocumented, "No integrated personal history documented",
                    "Ubuntu offers backup applications and filesystem capabilities. Its reviewed default desktop information does not document a personal-history layer tied to its recovery model.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "swap-health", "gauge", "Swap and hibernation health", "Sizing, resume identity and diagnostics", false,
                Cell(ComparisonLevel.FirstClass, "Capacity-aware policy + health UI",
                    "The installer sizes dedicated disk Swap with hibernation capacity in mind. Swap Control verifies the resume target, active capacity, UUID or path resolution and unsafe swapfile offsets.",
                    Source("Swap Control architecture", SwapDesign), Source("Installer design", InstallerDesign)),
                Cell(ComparisonLevel.NotDocumented, "No health dashboard documented",
                    "Zorin supports Linux Swap and hibernation mechanisms. Its reviewed public product information does not document a distribution-owned sizing and health dashboard of this scope.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.NotDocumented, "No health dashboard documented",
                    "Mint exposes standard Linux Swap and hibernation capabilities. Its reviewed public product information does not document an integrated sizing and health-verification control plane.",
                    Source("Linux Mint 22.3 release", MintRelease)),
                Cell(ComparisonLevel.NotDocumented, "No health dashboard documented",
                    "Ubuntu supplies the underlying kernel and installer mechanisms. Its reviewed default desktop information does not document a sizing and health UI with the same scope.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "app-model", "boxes", "Application model", "APT, Flatpak and Snap policy", false,
                Cell(ComparisonLevel.DefaultProvided, "APT system · Flatpak apps · no Snap",
                    "AnduinOS uses native APT packages for the system and Flatpak for sandboxed applications. snapd is conflicted, pinned out, unmounted and purged by product policy.",
                    Source("AnduinOS package architecture", AnduinPackages)),
                Cell(ComparisonLevel.DefaultProvided, "APT + Flatpak + Snap",
                    "Zorin deliberately supports a broad application estate: APT, Flatpak, Snap, AppImage, web apps and optional Windows application support.",
                    Source("Zorin OS technical details", ZorinDetails), Source("Zorin Windows App Support", ZorinWindowsApps)),
                Cell(ComparisonLevel.DefaultProvided, "APT + Flatpak · Snap blocked",
                    "Mint provides first-class APT and Flatpak integration and blocks Snap by default, while still allowing users to deliberately re-enable it.",
                    Source("Linux Mint 22.3 release", MintRelease)),
                Cell(ComparisonLevel.DefaultProvided, "APT + Snap by default",
                    "Ubuntu uses APT for the base system and Snap as a default application-delivery route. Flatpak remains available but is not the default Ubuntu path.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "telemetry", "eye-off", "System telemetry policy", "Diagnostics and remote news", false,
                Cell(ComparisonLevel.FirstClass, "Explicit zero-system-telemetry",
                    "AnduinOS removes or conflicts with Ubuntu reporting components such as whoopsie and disables Canonical remote MOTD news. The policy is expressed in packages rather than only in a privacy statement.",
                    Source("AnduinOS package architecture", AnduinPackages)),
                Cell(ComparisonLevel.NotDocumented, "No package-level policy documented",
                    "The reviewed public Zorin product information does not document a distribution-wide package-conflict and remote-news contract. This does not imply that Zorin profiles individual users.",
                    Source("Zorin OS technical details", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Privacy-oriented defaults",
                    "Linux Mint publishes privacy-oriented defaults. Its reviewed public material does not document the same package-level conflict and replacement contract for every Ubuntu reporting component.",
                    Source("Linux Mint 22.3 release", MintRelease)),
                Cell(ComparisonLevel.DefaultProvided, "Diagnostics with release controls",
                    "Ubuntu includes diagnostics and online-service components with release-specific controls. This is a broader product policy rather than AnduinOS's explicit zero-component contract.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "acceptance", "test-tube-diagonal", "Release quality assurance", "Public boot, install, reboot and desktop evidence", false,
                Cell(ComparisonLevel.FirstClass, "QEMU · QMP · AT-SPI",
                    "The black-box framework boots real ISOs, drives the GTK installer through semantic accessibility data and QEMU input, reboots the installed system and records screenshots, serial logs and contract evidence.",
                    Source("AnduinOS ISO acceptance framework", AnduinIso)),
                Cell(ComparisonLevel.DefaultProvided, "Release QA · public suite not documented",
                    "Zorin performs release engineering and quality assurance. Its reviewed public material does not document a distribution-level black-box suite publishing installer and desktop acceptance evidence.",
                    Source("Zorin OS 18.1 details", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Release QA",
                    "Mint performs release and ISO quality assurance, including fully QA-tested HWE images. Its reviewed public process does not document the AnduinOS QEMU/QMP/AT-SPI acceptance contract.",
                    Source("Linux Mint HWE QA", MintHwe)),
                Cell(ComparisonLevel.FirstClass, "Extensive image and release QA",
                    "Canonical operates extensive archive, image, hardware and release validation at upstream scale. Its implementation and evidence model differ from the AnduinOS acceptance framework.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease)))
        ];
    }

    private static ComparisonCell Cell(
        ComparisonLevel level,
        string summary,
        string detail,
        params ComparisonSource[] sources)
    {
        return new ComparisonCell(level, summary, detail, sources);
    }

    private static ComparisonSource Source(string label, string url)
    {
        return new ComparisonSource(label, url);
    }
}
