# OpenUPM Package Signing

> **Template note:** This is a template document. Replace `YOUR_PACKAGE_ID`
> (your reverse-domain package id, e.g. `com.company.package`) and
> `IvanMurzak/YOUR_REPO` (your `owner/repo` slug) with your real values when you
> adopt this package as a real one. The placeholders match those used in
> `.github/workflows/release.yml-sample`.

Unity 6.3 introduced a package-signature check that surfaces a trust warning for
unsigned UPM packages installed from third-party registries (including OpenUPM).
This document describes how a package built from this template signs its
`YOUR_PACKAGE_ID` package so the warning no longer appears in Unity 6.3+.

## How signing works

OpenUPM does **not** sign packages on behalf of authors — each package author runs
the signing flow in their own CI using a Unity organization's service account. The
signed `.tgz` is uploaded as a GitHub Release asset, and OpenUPM picks it up when
the package's listing has `trackingMode: githubRelease`.

References:
- <https://openupm.com/docs/signing-upm-packages.html>
- <https://openupm.com/blog/signing-upm-packages-with-openupm/>
- Reference workflow / repo layout: <https://github.com/openupm/com.example.signed-upm>

## What this template ships

The signing step is implemented as the `build-signed-upm-package` job in
[`.github/workflows/release.yml`](../.github/workflows/release.yml) (copied from
`release.yml-sample`). It runs in parallel with tests and builds on every
version-bump release commit, packs the package at
`Unity-Package/Assets/root/` with Unity's UPM CLI, verifies the resulting archive
contains `package/.attestation.p7m` and that its basename begins with
`YOUR_PACKAGE_ID-`, and uploads the signed `.tgz` as a `signed-upm-package`
workflow artifact.

The artifact is then consumed by the atomic publish step in `release-unity-plugin`,
which downloads every release asset (the installer `.unitypackage` and the signed
`.tgz`) and creates the GitHub Release + tag with all assets attached in a single
`softprops/action-gh-release@v2` call. There are no separate post-release publish
jobs — the release is created in a single step after all prerequisites pass; if any
prerequisite fails, no release is created. The `fail_on_unmatched_files: true`
option on the release action ensures the step hard-fails (rather than silently
publishing) if any of the asset globs match zero files.

### Signing is a hard gate on the release

`build-signed-upm-package` is **not** `continue-on-error`. If the three required
repo secrets (see below) are missing, or if `upm pack` / attestation verification
fails for any reason, the job exits non-zero, the release-creation jobs do not
run, and **no GitHub Release is created**. This is intentional: every public
release must ship the signed UPM tarball so OpenUPM (with the listing on
`trackingMode: githubRelease`) can surface the signed package without ever
race-publishing an unsigned git tag.

If you need to ship a release without signing, the correct action is to land a
follow-up PR that explicitly removes the gate — not to silently skip signing.

## One-time setup (repository owner)

These steps are operational, not code changes. The release pipeline cannot ship
a release until they are complete.

### 1. Create a Unity organization service account

A Unity organization is required to obtain UPM signing credentials (the
individual / personal Unity license cannot sign packages).

1. Go to the [Unity Cloud Dashboard](https://cloud.unity.com/) and either create
   an organization or use an existing one you own. Note its **Organization ID** —
   that's `UPM_ORG_ID`.
2. Inside the organization, go to **Administration → Service Accounts → Create
   service account**.
3. Grant the service account the **"Package Manager Package Signer"** role for the
   organization.
4. In the service account's **Keys** section, **Create key** — record the
   `Key ID` (`UPM_SERVICE_ACCOUNT_KEY_ID`), the `Key Secret`
   (`UPM_SERVICE_ACCOUNT_KEY_SECRET`), and the organization's `Org ID`
   (`UPM_ORG_ID`). The secret is shown only once.

### 2. Add the three GitHub repository secrets

In this repo's Settings → Secrets and variables → Actions, add:

| Secret name                       | Value                                |
| --------------------------------- | ------------------------------------ |
| `UPM_SERVICE_ACCOUNT_KEY_ID`      | Service account key ID               |
| `UPM_SERVICE_ACCOUNT_KEY_SECRET`  | Service account key secret           |
| `UPM_ORG_ID`                      | Unity organization ID                |

CLI equivalent:

```bash
gh secret set UPM_SERVICE_ACCOUNT_KEY_ID     --repo IvanMurzak/YOUR_REPO
gh secret set UPM_SERVICE_ACCOUNT_KEY_SECRET --repo IvanMurzak/YOUR_REPO
gh secret set UPM_ORG_ID                     --repo IvanMurzak/YOUR_REPO
```

### 3. Make sure the org is authorized to sign your namespace

Three things must line up, or `upm pack` fails with `User does not have permission
to sign package … with the provided credentials and organization`:

- The **service account must belong to the same organization** as `UPM_ORG_ID`.
  A key from a different org will authenticate but not be allowed to sign.
- That **organization must be authorized to sign your package's namespace** — the
  reverse-domain name in `package.json` (e.g. `com.company.package`, i.e.
  `YOUR_PACKAGE_ID`).
- A brand-new org will **not** be authorized for a namespace until it **claims**
  it. The UPM CLI can only *sign* a namespace the org already owns — it cannot
  *claim* one. To claim it, do a **one-time interactive sign in the Unity Editor**:
  Package Manager → select your package → **Export** → in the **Authoring Org**
  dropdown pick that organization → sign. After that single interactive sign, the
  CI service account can sign the same package automatically.

> The interactive Editor sign above is the only way to establish the org ↔
> namespace association. Do it once, from any machine, with an account that is a
> member of the signing organization.

### 4. File the OpenUPM listing change

If you publish via OpenUPM, OpenUPM's package listing for `YOUR_PACKAGE_ID` likely
starts with `trackingMode: git`, which makes OpenUPM pack and serve unsigned
tarballs from the repository's git tags. To make OpenUPM serve the signed tarball
that the workflow now uploads, the listing must be flipped to
`trackingMode: githubRelease`.

The listing lives in the [openupm/openupm](https://github.com/openupm/openupm)
repository at `data/packages/YOUR_PACKAGE_ID.yml`. Open a PR there changing:

```yaml
trackingMode: git
```

to:

```yaml
trackingMode: githubRelease
```

Per the OpenUPM blog, switch `trackingMode` to `githubRelease` **before** the
first signed release ships, so OpenUPM does not race-publish the unsigned git
tag in parallel.

Also set `githubReleaseAssetName` so OpenUPM picks the signed tarball by
filename prefix rather than guessing from the asset list (the release also ships
the installer `.unitypackage`, and may add more assets later, so the prefix guard
prevents a future-breaking failure mode):

```yaml
githubReleaseAssetName: 'YOUR_PACKAGE_ID-'
```

## Verifying signing worked

After the next release ships:

1. Go to the release page for the new version and confirm a
   `YOUR_PACKAGE_ID-<version>.tgz` asset is attached alongside the
   `.unitypackage`. The single-step publish runs only after the signed tarball is
   built and verified, so a successful release run should always include the
   signed asset.
2. Inspect the tarball locally to confirm it contains the signing attestation:

   ```bash
   curl -fsSL -o package.tgz \
     https://github.com/IvanMurzak/YOUR_REPO/releases/download/<version>/YOUR_PACKAGE_ID-<version>.tgz
   tar -tzf package.tgz | grep '\.attestation\.p7m$'
   # expected: package/.attestation.p7m
   ```

3. Once the OpenUPM listing change merges, install the package in Unity 6.3+
   from OpenUPM and confirm the unsigned-package warning no longer appears.

## Troubleshooting

- **`build-signed-upm-package` fails with `UPM signing secrets are not configured`** —
  the three repo secrets above have not been set (or were set on the wrong repo).
  Complete the "One-time setup" steps above. The release pipeline is hard-gated
  on these secrets; until they are configured no release will ship.
- **`upm pack` fails with `User does not have permission to sign package …`** —
  one of the three alignment conditions in step 3 is not met: the service account
  is in a different org than `UPM_ORG_ID`, the org is not authorized for your
  namespace, or the namespace was never claimed via a one-time interactive Editor
  sign. Re-check step 3.
- **`upm pack` fails with a generic authentication error** — the service account
  key is invalid or lacks the **Package Manager Package Signer** role. Regenerate
  the key in the Unity org dashboard and re-set the GitHub secrets.
- **The release contains the `.tgz` but Unity 6.3 still shows the warning** —
  the OpenUPM listing is still on `trackingMode: git` (OpenUPM is serving the
  unsigned git-packed version, not the release asset). File the
  `openupm/openupm` PR described above.
