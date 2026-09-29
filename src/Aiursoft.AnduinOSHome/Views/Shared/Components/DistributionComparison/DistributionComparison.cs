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
    private const string SnapshotRecoveryScope = AnduinPackages + "/blob/master/anduinos-btrfs-snapshots-manager/docs/RECOVERY-SCOPE.md";
    private const string ZorinDetails = "https://zorin.com/os/details/";
    private const string ZorinReinstall = "https://help.zorin.com/docs/getting-started/replace-your-zorin-os-installation/";
    private const string ZorinNvidia = "https://help.zorin.com/docs/hardware/activate-nvidia-drivers/";
    private const string ZorinWindowsApps = "https://help.zorin.com/docs/apps-games/windows-app-support/";
    private const string MintRelease = "https://blog.linuxmint.com/?p=4981";
    private const string MintHwe = "https://blog.linuxmint.com/?p=5050";
    private const string MintWayland = "https://blog.linuxmint.com/?p=5046";
    private const string MintTimeshift = "https://linuxmint-installation-guide.readthedocs.io/en/latest/timeshift.html";
    private const string MintTimeshiftSource = "https://github.com/linuxmint/timeshift/blob/master/README.md";
    private const string UbuntuRelease = "https://documentation.ubuntu.com/release-notes/26.04/summary-for-lts-users/";
    private const string UbuntuDesktop = "https://ubuntu.com/download/desktop";
    private const string UbuntuDesktopInstall = "https://ubuntu.com/desktop/docs/en/26.04/tutorial/install-ubuntu-desktop/";
    private const string UbuntuCoreRecovery = "https://documentation.ubuntu.com/core/explanation/recovery-modes/";
    private const string UbuntuSecureBoot = "https://documentation.ubuntu.com/security/docs/security-features/platform-protections/secure-boot/";
    private const string UbuntuLiveBuild = "https://launchpad.net/ubuntu/+source/livecd-rootfs/26.04.32";
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
                "platform", "crown", "System foundation", "Ubuntu version and Linux kernel", true,
                Cell(ComparisonLevel.FirstClass, "Ubuntu 26.04 LTS · Linux 7.0",
                    "AnduinOS 2 is based on Ubuntu 26.04 LTS and its Linux 7.0 kernel. The kernel connects software to your hardware. AnduinOS keeps this foundation through installation and normal updates, rather than changing it only in the installation image.",
                    Source("AnduinOS packages and source code", AnduinPackages)),
                Cell(ComparisonLevel.DefaultProvided, "Ubuntu 24.04 · Linux 6.17",
                    "Zorin OS 18.1 uses Ubuntu 24.04 LTS and Linux 6.17. Its desktop and system software follow the Ubuntu 24.04 base; a newer kernel does not by itself bring the desktop features of Ubuntu 26.04 or GNOME 50.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Ubuntu 24.04 · Linux 6.14 or 7.0 HWE",
                    "Linux Mint 22.3 launched with Linux 6.14. It also offers tested HWE images with Linux 7.0 for newer hardware. HWE means hardware enablement: check which installation image you download, because the included kernel differs.",
                    Source("Linux Mint 22.3 release notes", MintRelease), Source("Mint images for newer hardware", MintHwe)),
                Cell(ComparisonLevel.FirstClass, "Ubuntu 26.04 LTS · Linux 7.0",
                    "Ubuntu 26.04 LTS provides the Linux 7.0 kernel and five years of standard LTS maintenance. LTS means long-term support. It is the Ubuntu release on which AnduinOS 2 is based.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "wayland", "monitor-up", "Wayland desktop", "How apps display windows on your screen", true,
                Cell(ComparisonLevel.FirstClass, "Wayland desktop only",
                    "Wayland handles how applications display windows and receive input. AnduinOS tests its desktop on Wayland and uses XWayland to run older X11 applications. It does not include an Xorg desktop session, so you cannot switch to Xorg to work around a Wayland-specific issue.",
                    Source("AnduinOS packages and source code", AnduinPackages)),
                Cell(ComparisonLevel.DefaultProvided, "Wayland by default; Xorg available",
                    "Zorin defaults to Wayland but also offers an Xorg session at login. Switching to Xorg can help with applications or graphics drivers that have trouble under Wayland, while allowing users to keep the newer session as their usual choice.",
                    Source("Zorin NVIDIA driver help", ZorinNvidia)),
                Cell(ComparisonLevel.Experimental, "X11 by default; Wayland experimental",
                    "Linux Mint 22.3 uses Cinnamon on X11 by default. Its Wayland session remains experimental in this version. Mint has announced full support for both X11 and Wayland in the next Cinnamon release; that is not the same as support already shipping in 22.3.",
                    Source("Mint Wayland development updates", MintWayland)),
                Cell(ComparisonLevel.FirstClass, "GNOME on Wayland only",
                    "Ubuntu 26.04 runs the GNOME desktop on Wayland and no longer offers an Xorg desktop session. XWayland remains available to run older applications written for X11, so a Wayland-only desktop does not exclude all older Linux apps.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "architectures", "cpu", "Supported processors", "Available desktop installation images", true,
                Cell(ComparisonLevel.FirstClass, "AMD64 + ARM64",
                    "AnduinOS provides AMD64 and ARM64 installation images. AMD64 covers 64-bit Intel and AMD PCs; ARM64 is a different processor family. Both are included in the installer, signed UEFI boot files and automated tests. ARM64 support does not mean every ARM device can boot the image.",
                    Source("AnduinOS images and tests", AnduinIso), Source("How the AnduinOS installer works", InstallerDesign)),
                Cell(ComparisonLevel.Unavailable, "Official x86-64 images",
                    "Zorin OS 18.1 offers desktop images for 64-bit Intel and AMD processors, also called x86-64 or AMD64. Its official downloads do not list an ARM64 desktop image.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.Unavailable, "Official x86-64 images",
                    "Linux Mint 22.3 offers Cinnamon, MATE and Xfce images for 64-bit Intel and AMD processors. Its official desktop downloads do not offer an ARM64 edition.",
                    Source("Linux Mint 22.3 release notes", MintRelease)),
                Cell(ComparisonLevel.FirstClass, "AMD64 + ARM64",
                    "Ubuntu 26.04 offers official desktop downloads for AMD64 and ARM64. Select the image for your processor and check device compatibility: support for an architecture does not guarantee support for every machine using it.",
                    Source("Ubuntu Desktop downloads", UbuntuDesktop))),

            new ComparisonItem(
                "secure-boot", "shield-check", "Secure Boot", "Trusted startup and driver signatures", true,
                Cell(ComparisonLevel.FirstClass, "Setup and driver-signature checks",
                    "Secure Boot checks signatures on startup software. AnduinOS includes configuration and status tools in the installer, first-run setup and Driver Center on AMD64 and ARM64. You can manage MOK keys, which authorize additional signing certificates, and check signatures on drivers built through DKMS.",
                    Source("How the AnduinOS installer works", InstallerDesign), Source("AnduinOS packages and source code", AnduinPackages)),
                Cell(ComparisonLevel.Supported, "Uses Ubuntu Secure Boot",
                    "Zorin supports Secure Boot through its Ubuntu base. Its troubleshooting guide also describes cases where users may need to disable Secure Boot or switch to Xorg when NVIDIA drivers do not work. Support can depend on the hardware and installed driver.",
                    Source("Zorin NVIDIA driver help", ZorinNvidia)),
                Cell(ComparisonLevel.Supported, "Uses Ubuntu-signed boot components",
                    "Mint inherits Ubuntu's signed bootloader and kernel components. These let supported machines start with Secure Boot enabled. Mint also documents workarounds for Secure Boot violations; driver and firmware compatibility still matter.",
                    Source("Ubuntu Secure Boot documentation", UbuntuSecureBoot)),
                Cell(ComparisonLevel.FirstClass, "Signed boot components and driver tools",
                    "Ubuntu uses a Microsoft-signed shim to start Canonical-signed GRUB and kernels on AMD64 and ARM64. MOK enrollment lets users trust additional certificates, and DKMS signing supports locally built drivers. These components form the chain checked during Secure Boot.",
                    Source("Ubuntu Secure Boot documentation", UbuntuSecureBoot))),

            new ComparisonItem(
                "btrfs", "database", "Default filesystem", "How the installed system stores files", true,
                Cell(ComparisonLevel.FirstClass, "Btrfs by default; ext4 optional",
                    "Btrfs supports snapshots: saved views of files that can be used for recovery. AnduinOS selects it by default and separates system files, home folders, logs, containers and virtual machines into subvolumes so recovery can treat them differently. You can choose ext4 instead, without the integrated Btrfs snapshot features.",
                    Source("AnduinOS storage and recovery design", BtrfsDesign), Source("How the AnduinOS installer works", InstallerDesign)),
                Cell(ComparisonLevel.NotDocumented, "ext4 by default",
                    "Zorin's default installation uses ext4, a widely used Linux filesystem. The reviewed documentation does not establish a built-in Btrfs layout with the same integrated recovery features. This does not rule out configuring other storage tools yourself.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.Supported, "ext4 by default; Btrfs selectable",
                    "Mint defaults to ext4 but allows Btrfs. Timeshift can use Btrfs snapshots with a suitable layout. Choosing Btrfs alone does not set up the same separation of system files and recovery data used by AnduinOS.",
                    Source("Mint guide to system snapshots", MintTimeshift)),
                Cell(ComparisonLevel.Supported, "ext4 by default; other setups possible",
                    "Ubuntu's guided desktop installation defaults to ext4. Other filesystem layouts can be configured. It does not provide AnduinOS's Btrfs subvolume layout and recovery integration as its default desktop setup.",
                    Source("Ubuntu Desktop downloads", UbuntuDesktop))),

            new ComparisonItem(
                "recovery", "history", "System backup and recovery", "Getting back to a working system", true,
                Cell(ComparisonLevel.FirstClass, "Automatic snapshots; recovery at startup",
                    "On Btrfs, AnduinOS creates system snapshots around APT software changes and makes recovery available through the boot menu. Dracut, GRUB and the snapshot manager work together to restore the selected state and confirm a successful boot before cleanup. Snapshots on the same disk do not replace an external backup against disk failure.",
                    Source("AnduinOS Snapshot Manager", SnapshotDesign), Source("AnduinOS storage and recovery design", BtrfsDesign)),
                Cell(ComparisonLevel.NotDocumented, "Built-in snapshot recovery not confirmed",
                    "Zorin provides recovery options, but the reviewed sources do not confirm automatic software-change snapshots linked to boot-menu recovery and boot confirmation. Other backup tools may be installed or configured separately.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "System snapshots with Timeshift",
                    "Timeshift creates system snapshots, supports schedules and manages how many snapshots are kept. You can use it to restore an earlier system state. Its recovery process differs from AnduinOS's integration with software changes and startup confirmation.",
                    Source("Mint guide to system snapshots", MintTimeshift)),
                Cell(ComparisonLevel.NotDocumented, "Built-in snapshot recovery not confirmed",
                    "Ubuntu offers recovery mechanisms and optional backup tools. The reviewed desktop documentation does not confirm a default feature combining Btrfs snapshots, APT changes and startup confirmation. Users can build their own backup and recovery setup.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "memory", "memory-stick", "Memory management", "What happens when RAM fills up", true,
                Cell(ComparisonLevel.FirstClass, "Memory compression, Swap and a settings app",
                    "Zram compresses data in RAM before lower-priority disk Swap is used. AnduinOS configures LZ4 Zram at 50% of RAM and sizes a disk Swap partition during installation. Swap Control manages Zram and optional Zswap, runs stress tests and checks hibernation settings. Compression does not replace physical RAM.",
                    Source("How Swap Control works", SwapDesign), Source("How the AnduinOS installer works", InstallerDesign)),
                Cell(ComparisonLevel.Supported, "Follows Ubuntu memory management",
                    "Zorin uses the memory and Swap features of its Ubuntu 24.04 base. The reviewed sources do not confirm a bundled app combining Zram, Zswap, stress testing and hibernation checks. Individual settings can still be managed with Linux tools.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.NotDocumented, "Standard Linux memory and Swap tools",
                    "Mint provides standard Linux memory management and Swap support. The reviewed release documentation does not confirm a bundled app that combines compressed memory, stress tests and hibernation checks in one place.",
                    Source("Linux Mint 22.3 release notes", MintRelease)),
                Cell(ComparisonLevel.DefaultProvided, "General-purpose memory and Swap defaults",
                    "Ubuntu supplies Linux memory management and Swap features with general desktop defaults. Its default setup differs from AnduinOS's combination of preconfigured Zram, a dedicated Swap partition and a graphical management tool.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "native-tools", "panels-top-left", "Built-in system tools", "A consistent look for everyday settings", true,
                Cell(ComparisonLevel.FirstClass, "GTK4 across our graphical system tools",
                    "The Installer, Control Panel, Appearance app, first-run setup, Driver Center, Secure Boot Toolkit, Swap Control and Snapshot Manager share GTK4 and libadwaita. These interface libraries help keep controls and resizing behavior consistent. This applies to AnduinOS's own graphical system tools, not every third-party application.",
                    Source("AnduinOS packages and source code", AnduinPackages)),
                Cell(ComparisonLevel.DefaultProvided, "GNOME apps and Zorin tools",
                    "Zorin combines GNOME applications and extensions with its own tools on the Ubuntu 24.04 base. These use a mix of interface technologies rather than one GTK4/libadwaita toolkit across all system tools.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Cinnamon and XApp tools",
                    "Mint maintains Cinnamon and XApp tools with their own interface design. Major administration tools mainly use GTK3 and XApp. GTK4 applications can still run; the toolkit used by a settings app does not determine which apps users can install.",
                    Source("Linux Mint 22.3 release notes", MintRelease)),
                Cell(ComparisonLevel.DefaultProvided, "GNOME apps and Ubuntu tools",
                    "Ubuntu includes modern GNOME applications using GTK4 and libadwaita. Its installer and other Ubuntu-specific tools also use different technologies, so not every system interface is built with the same toolkit.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease), Source("What's new in GNOME 50", GnomeRelease))),

            new ComparisonItem(
                "modern-graphics", "sparkles", "Display features", "HDR, variable refresh rate and scaling", false,
                Cell(ComparisonLevel.FirstClass, "GNOME 50 display features",
                    "GNOME 50 brings HDR screen sharing, updated color management, improved variable refresh rate and fractional scaling. HDR supports a wider brightness range; VRR adjusts refresh timing; fractional scaling makes content fit high-resolution screens. Availability depends on the display, graphics driver and application.",
                    Source("What's new in GNOME 50", GnomeRelease)),
                Cell(ComparisonLevel.Supported, "Follows Ubuntu 24.04's GNOME",
                    "Zorin's GNOME base follows Ubuntu 24.04. It offers Wayland display features but predates the GNOME 50 improvements to HDR, variable refresh rate and color management. The specific feature available also depends on your monitor and driver.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.Experimental, "Wayland still experimental",
                    "Cinnamon 6.6 in Mint 22.3 mainly uses its established X11 desktop. Wayland support is still experimental, with further improvements planned for the next release. Do not assume GNOME's HDR or scaling features apply to Cinnamon.",
                    Source("Mint Wayland development updates", MintWayland)),
                Cell(ComparisonLevel.FirstClass, "GNOME 50 display features",
                    "Ubuntu 26.04 ships GNOME 50 with its Wayland display improvements, including optimized fractional scaling. HDR, variable refresh rate and color-management features depend on compatible graphics hardware, drivers, displays and applications.",
                    Source("Ubuntu Desktop downloads", UbuntuDesktop), Source("What's new in GNOME 50", GnomeRelease))),

            new ComparisonItem(
                "nvidia", "cpu", "NVIDIA graphics", "Driver setup and Wayland support", false,
                Cell(ComparisonLevel.FirstClass, "Wayland support and Driver Center",
                    "AnduinOS combines Ubuntu 26.04 drivers with GNOME 50's frame-timing improvements. Driver Center helps manage drivers alongside Secure Boot and DKMS signature checks. The installed driver and GPU still need to support the features you want to use.",
                    Source("AnduinOS packages and source code", AnduinPackages), Source("What's new in GNOME 50", GnomeRelease)),
                Cell(ComparisonLevel.Supported, "NVIDIA drivers; Xorg available",
                    "Zorin offers a Live boot option for modern NVIDIA hardware and tools to install proprietary drivers. Its troubleshooting guidance also covers switching to Xorg or changing Secure Boot settings when a driver does not work.",
                    Source("Zorin NVIDIA driver help", ZorinNvidia)),
                Cell(ComparisonLevel.Experimental, "Wayland support still being improved",
                    "Mint has announced more NVIDIA Wayland improvements for the next Cinnamon release. In Mint 22.3, the Wayland session remains experimental. This concerns Wayland support, not whether NVIDIA hardware can be used at all.",
                    Source("Mint Wayland development updates", MintWayland)),
                Cell(ComparisonLevel.FirstClass, "NVIDIA supported on Wayland",
                    "Ubuntu 26.04 lists NVIDIA graphics as supported in its Wayland desktop session. Use an appropriate driver for your GPU; support for the desktop does not guarantee every feature on every card.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "dracut", "workflow", "System startup", "Preparing the files needed to boot", false,
                Cell(ComparisonLevel.FirstClass, "Dracut throughout",
                    "Before Linux opens your desktop, a small startup environment called initramfs loads drivers and locates the system disk. Dracut builds that environment.\n\nAnduinOS uses Dracut for both the installed system and the Live USB. Its core package prevents incompatible initramfs-tools, Casper and finalrd packages from being installed alongside it. Generated boot images are checked, and package safeguards prevent other generators from silently replacing them. This unifies maintenance; it does not by itself guarantee faster startup.",
                    Source("AnduinOS packages and source code", AnduinPackages), Source("AnduinOS images and tests", AnduinIso)),
                Cell(ComparisonLevel.DefaultProvided, "initramfs-tools and Casper",
                    "An initramfs is the small environment that loads drivers and finds the system before normal startup. Zorin uses initramfs-tools from Ubuntu 24.04 to generate it, with Casper for the Live USB environment. This is a different boot-tool choice from Dracut, not the absence of boot support.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Ubuntu 24.04 startup tools",
                    "Mint 22.3 uses the Ubuntu 24.04 generation of initramfs and Live tools. These prepare the drivers and files needed before the desktop starts. The reviewed sources do not describe a default migration to Dracut across both installed and Live systems.",
                    Source("Linux Mint 22.3 release notes", MintRelease)),
                Cell(ComparisonLevel.DefaultProvided, "Dracut by default after installation",
                    "Ubuntu 26.04 uses Dracut by default to generate the installed system's initramfs, the small environment used during early startup. initramfs-tools remains supported. The installation USB is a separate case: its Live environment still uses initramfs-tools and Casper.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "live-layers", "layers-3", "Live USB startup", "Trying the desktop without installing it", false,
                Cell(ComparisonLevel.FirstClass, "Live USB migrated to Dracut",
                    "A Live USB lets you try the desktop without installing it on your disk. It needs a startup environment to load the compressed system image. AnduinOS now builds this with Dracut, the same tool used by the installed system.\n\nDracut's dmsquash-live loads the image. AnduinOS Live Layers manages a writable layer for your changes, either temporary or saved on the USB for later use. It also keeps the installation files available and supports repair of expanded USB layouts. Using one boot tool helps maintain and troubleshoot both environments; it is not a claim that every USB starts faster.",
                    Source("AnduinOS images and tests", AnduinIso), Source("AnduinOS packages and source code", AnduinPackages)),
                Cell(ComparisonLevel.Supported, "Ubuntu-based Live USB tools",
                    "Zorin lets you try its desktop from a USB before installing. Its Live environment uses the Ubuntu-derived Casper and initramfs-tools approach. Persistent storage saves changes across boots when configured; it is separate from simply being able to start a Live session.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.Supported, "Mint Live USB environment",
                    "Mint provides a desktop you can try from a USB without installing to disk. Its Live tools follow the Ubuntu 24.04 base. Saving changes across restarts requires a suitable persistence setup; the reviewed sources do not describe migration to AnduinOS's Dracut Live Layers.",
                    Source("Linux Mint 22.3 release notes", MintRelease)),
                Cell(ComparisonLevel.Supported, "Still uses initramfs-tools and Casper",
                    "A Live environment is the desktop you start from an installation USB before installing. Ubuntu 26.04 still builds that startup environment with initramfs-tools and Casper, which locate and load the system image.\n\nThis differs from the installed Ubuntu system, which defaults to Dracut. AnduinOS has also moved the Live environment to Dracut, so its USB and installed system use the same boot tool. The difference here is migration coverage, not whether Ubuntu can run from a USB.",
                    Source("Ubuntu Live image build notes", UbuntuLiveBuild), Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "installer", "hard-drive-download", "Installer", "Planning and installing your system", false,
                Cell(ComparisonLevel.FirstClass, "Declarative installer",
                    "Declarative means the installer first describes the intended result: disks, partitions and system settings. A GTK4 interface creates a typed installation plan without administrator privileges. A separate privileged component checks that plan again and builds the commands itself. The interface cannot pass arbitrary shell commands or paths for privileged execution.",
                    Source("How the AnduinOS installer works", InstallerDesign)),
                Cell(ComparisonLevel.DefaultProvided, "Ubuntu-based graphical installer",
                    "Zorin provides a graphical installer based on Ubuntu's installation tools. It guides users through installing the system. The reviewed sources do not establish the same separation between a typed plan and a fixed privileged executor used by AnduinOS.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Mint graphical installer",
                    "Mint provides its established graphical installation tool. Users choose settings through an interface that installs the system. Its design differs from AnduinOS's declarative plan and separate executor; a different architecture alone does not imply an unsafe installer.",
                    Source("Linux Mint 22.3 release notes", MintRelease)),
                Cell(ComparisonLevel.DefaultProvided, "Installer with a Flutter interface",
                    "Ubuntu's desktop installer uses a Flutter interface with a separate backend to perform installation. It provides a modern graphical setup experience. Its implementation differs from AnduinOS's GTK4 interface and typed installation plan.",
                    Source("Ubuntu Desktop downloads", UbuntuDesktop))),

            new ComparisonItem(
                "package-engineering", "package-check", "System packaging", "How system packages are built and organized", false,
                Cell(ComparisonLevel.FirstClass, "System packages built with APKG",
                    "APKG builds AnduinOS packages from .aosproj project files. Each project records supported releases and processors, dependencies, conflicts, files, services, AppStream app information and build checks. Metapackages group these packages into the system you install. This is mainly useful to contributors maintaining or rebuilding AnduinOS.",
                    Source("AnduinOS packages and source code", AnduinPackages)),
                Cell(ComparisonLevel.DefaultProvided, "Debian packages and Zorin repositories",
                    "Zorin uses Debian and Ubuntu packaging with its own software repositories and package selection. These provide the components of the finished desktop. It uses its own packaging process rather than AnduinOS's APKG project format.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Debian packages and Mint build tools",
                    "Mint uses Debian packages, metapackages that group dependencies, and its own project tools to build the desktop. This is an established way to organize a distribution; using a different project format does not imply missing user features.",
                    Source("Linux Mint 22.3 release notes", MintRelease)),
                Cell(ComparisonLevel.FirstClass, "Package build and archive testing tools",
                    "Ubuntu uses seeds to select packages, metapackages to group dependencies, and archive infrastructure with automated tests. These organize and validate software across many images and devices. The tooling operates at a much larger scale than AnduinOS's package projects.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "apt-snapshots", "package-plus", "Protection during software changes", "Snapshots around installations and updates", false,
                Cell(ComparisonLevel.FirstClass, "Automatic snapshots before and after changes",
                    "On Btrfs, hooks in APT create snapshots before and after installing, updating or removing system packages. Each snapshot records package state and follows the system's retention and recovery settings. This protects APT changes; it does not mean every Flatpak or personal-file change creates a system snapshot.",
                    Source("AnduinOS Snapshot Manager", SnapshotDesign)),
                Cell(ComparisonLevel.NotDocumented, "Automatic software snapshots not confirmed",
                    "The reviewed Zorin documentation does not confirm default snapshots before and after each APT software change. Users may configure backup tools separately. This describes the built-in automation we could verify, not whether backups are possible.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.Supported, "Manual snapshot advised before upgrades",
                    "Mint recommends creating a Timeshift snapshot before a release upgrade so you can restore the earlier system if needed. That manual safeguard differs from automatically taking snapshots around every APT package operation.",
                    Source("Mint guide to system snapshots", MintTimeshift)),
                Cell(ComparisonLevel.NotDocumented, "Automatic software snapshots not confirmed",
                    "Ubuntu supports optional backup and snapshot tools. Its reviewed default desktop setup does not confirm automatic snapshots before and after every APT operation. You can arrange your own backup process before changing system software.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "personal-history", "folder-clock", "File history", "Finding and restoring earlier file versions", false,
                Cell(ComparisonLevel.FirstClass, "Browse and restore earlier file versions",
                    "On Btrfs, Personal Files snapshots keep home-folder history separately from system snapshots. Snapshot Manager and the Files integration let you browse older versions and restore selected files without rolling back the operating system. Files changed after the latest snapshot may not have a saved version; keep external backups for disk failure.",
                    Source("AnduinOS Snapshot Manager", SnapshotDesign), Source("AnduinOS storage and recovery design", BtrfsDesign)),
                Cell(ComparisonLevel.NotDocumented, "Built-in file history not confirmed",
                    "The reviewed sources do not confirm built-in file-version history integrated with Zorin's filesystem and file manager. Separate backup applications can provide other ways to protect personal files.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.Unavailable, "Personal files need a separate backup",
                    "Timeshift is designed to restore system state, not serve as a personal-file backup tool. Use a separate backup method for documents and other personal data. The presence of system snapshots should not be mistaken for saved versions of your files.",
                    Source("Mint guide to system snapshots", MintTimeshift)),
                Cell(ComparisonLevel.NotDocumented, "Built-in file history not confirmed",
                    "Ubuntu offers backup applications and filesystem tools. The reviewed default desktop setup does not confirm the same integrated file-history feature in the file manager. Personal backups can still be configured separately.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "factory-reset", "rotate-ccw", "Factory reset", "Returning to the freshly installed system", false,
                Cell(ComparisonLevel.FirstClass, "Built in with Btrfs snapshots",
                    "After a Btrfs installation is configured, AnduinOS saves a protected New OS snapshot and an initial home-folder snapshot. Open Factory Reset in Control Panel: it checks the recovery snapshots, saves a safety snapshot and restores the initial system on reboot, without an installation USB. Personal files are kept by default; restoring the initial home state is a separate choice. This requires the original snapshots and does not work on ext4. It is recovery, not secure erasure for selling a device.",
                    Source("AnduinOS Snapshot Manager", SnapshotDesign), Source("AnduinOS Snapshot Manager", SnapshotRecoveryScope)),
                Cell(ComparisonLevel.NotDocumented, "Reinstall using a USB",
                    "You can return to a fresh Zorin installation using an installation USB. Back up personal files first and review the disk choices carefully. This is a reinstall, rather than restoring an installation-time snapshot through the AnduinOS reset tool.",
                    Source("Zorin reinstallation guide", ZorinReinstall)),
                Cell(ComparisonLevel.NotDocumented, "Restore a snapshot or reinstall",
                    "Timeshift can restore a system snapshot you previously created. Returning to a freshly installed state requires a suitable saved snapshot or a reinstall. A routine system snapshot is not necessarily a protected copy of the original installation, and personal files require separate care.",
                    Source("Mint guide to system snapshots", MintTimeshift), Source("Mint guide to system snapshots", MintTimeshiftSource)),
                Cell(ComparisonLevel.NotDocumented, "Reinstall using a USB",
                    "An Ubuntu installation USB can be used to install a fresh system. Back up your files and check the selected disk and partitions first. Ubuntu Core has a separate factory-reset feature, but it is a different product from the Ubuntu Desktop edition compared here.",
                    Source("Ubuntu Desktop installation guide", UbuntuDesktopInstall), Source("Ubuntu Core recovery options", UbuntuCoreRecovery))),

            new ComparisonItem(
                "swap-health", "gauge", "Swap and hibernation", "Checking space and resume settings", false,
                Cell(ComparisonLevel.FirstClass, "Automatic Swap sizing and hibernation checks",
                    "Swap is disk space used when RAM is under pressure; hibernation also needs space to save memory before powering off. The installer sizes a dedicated Swap partition with this in mind. Swap Control checks capacity, the resume target and Swap file offsets. Enough Swap alone does not enable hibernation: hardware and resume settings must also work.",
                    Source("How Swap Control works", SwapDesign), Source("How the AnduinOS installer works", InstallerDesign)),
                Cell(ComparisonLevel.NotDocumented, "Hibernation check app not confirmed",
                    "Zorin supports Linux Swap and hibernation mechanisms. The reviewed sources do not confirm a bundled tool that checks sizing, the resume location and configuration together. Hibernation may require manual setup and compatible hardware.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.NotDocumented, "Hibernation check app not confirmed",
                    "Mint provides standard Linux Swap and hibernation capabilities. The reviewed documentation does not confirm a bundled app for combined capacity and resume checks. Availability still depends on the machine and its configuration.",
                    Source("Linux Mint 22.3 release notes", MintRelease)),
                Cell(ComparisonLevel.NotDocumented, "Hibernation check app not confirmed",
                    "Ubuntu provides the underlying kernel and installation tools for Swap. The reviewed desktop setup does not confirm a built-in app combining Swap sizing and hibernation diagnostics. Check hardware support and resume configuration before relying on hibernation.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "app-model", "boxes", "Installing applications", "APT, Flatpak and Snap defaults", false,
                Cell(ComparisonLevel.DefaultProvided, "APT for the system; Flatpak for apps",
                    "APT manages native system packages, while Flatpak supplies sandboxed applications. AnduinOS removes snapd and blocks its installation through package conflicts and APT preferences. This is a deliberate default software choice, not a claim that every application is available as a Flatpak.",
                    Source("AnduinOS packages and source code", AnduinPackages)),
                Cell(ComparisonLevel.DefaultProvided, "APT + Flatpak + Snap",
                    "Zorin supports APT, Flatpak and Snap, alongside AppImage and web apps. Optional Windows App Support can run some Windows software, but compatibility depends on the application. The aim is to offer several ways to install software.",
                    Source("Zorin OS specifications", ZorinDetails), Source("Zorin Windows App Support", ZorinWindowsApps)),
                Cell(ComparisonLevel.DefaultProvided, "APT + Flatpak; Snap blocked by default",
                    "Mint integrates APT and Flatpak and blocks Snap by default. Users can deliberately change that setting and enable Snap. The default reflects Mint's software policy rather than an inability to run Snap at all.",
                    Source("Linux Mint 22.3 release notes", MintRelease)),
                Cell(ComparisonLevel.DefaultProvided, "APT + Snap by default",
                    "Ubuntu uses APT for system packages and Snap for part of its default application offering. Flatpak can be installed separately, but is not included as the default desktop application route.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "telemetry", "eye-off", "System data collection", "Diagnostic reporting and privacy settings", false,
                Cell(ComparisonLevel.FirstClass, "System telemetry components disabled",
                    "AnduinOS removes or blocks Ubuntu reporting components such as whoopsie and disables Canonical's online terminal news. These choices are applied through system packages. They concern operating-system components; separately installed apps and websites can have their own data-collection settings.",
                    Source("AnduinOS packages and source code", AnduinPackages)),
                Cell(ComparisonLevel.NotDocumented, "Not confirmed",
                    "The reviewed sources do not establish an equivalent package-level block on Ubuntu reporting components and online terminal news. That is not enough to conclude whether Zorin collects personal data. Check its privacy information and the settings of the applications you use.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Privacy-oriented defaults",
                    "Mint describes privacy-oriented defaults. The reviewed sources do not establish the same package-by-package blocking rules used by AnduinOS. This comparison of implementation does not imply that Mint collects personal information.",
                    Source("Linux Mint 22.3 release notes", MintRelease)),
                Cell(ComparisonLevel.DefaultProvided, "Diagnostic features with configurable settings",
                    "Ubuntu includes diagnostic and online-service components whose controls depend on the release. Review the privacy options offered during setup and in system settings. These defaults differ from removing or blocking the components in AnduinOS.",
                    Source("Ubuntu 26.04 release notes", UbuntuRelease))),

            new ComparisonItem(
                "acceptance", "test-tube-diagonal", "Public automated tests", "Installation, startup and desktop checks", false,
                Cell(ComparisonLevel.FirstClass, "Published install, boot and desktop reports",
                    "Automated tests boot real installation images in QEMU virtual machines, run the installer, restart the installed system and check the desktop. QMP controls the virtual machine; AT-SPI exposes interface controls so tests can interact with them. Published results include screenshots and logs. Virtual-machine tests do not cover every physical device.",
                    Source("AnduinOS images and tests", AnduinIso)),
                Cell(ComparisonLevel.DefaultProvided, "Similar public test reports not found",
                    "Zorin performs release testing. In the official sources reviewed for this comparison, we did not find a comparable public set of automated installer and desktop reports. This concerns what users can inspect publicly, not whether Zorin tests its releases internally.",
                    Source("Zorin OS specifications", ZorinDetails)),
                Cell(ComparisonLevel.DefaultProvided, "Release testing",
                    "Mint tests releases and installation images, including its HWE images for newer hardware. The reviewed public sources did not identify the same kind of published automated installation and desktop reports as AnduinOS. Different reporting does not mean no quality testing.",
                    Source("Mint images for newer hardware", MintHwe)),
                Cell(ComparisonLevel.FirstClass, "Extensive image and release testing",
                    "Canonical tests software archives, installation images, hardware and releases at a broad scale. Its testing and reporting systems differ from AnduinOS's QEMU-based installer and desktop tests. The comparison is about available reports, not a claim that one test framework covers all quality concerns.",
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
