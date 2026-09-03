pluginManagement {
    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
    }
}
dependencyResolutionManagement {
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        mavenCentral()
        // TIDAK ada jcenter() di sini dengan sengaja — JCenter sudah
        // dimatikan TOTAL sejak Februari 2022 (semua request ke situ selalu
        // gagal). Sempat ditambahkan untuk "memperbaiki" resolusi dependency
        // br.com.devsrsouza.compose.icons:tabler:0.2.0, tapi itu tidak akan
        // pernah berhasil — dependency itu sudah dihapus dari
        // app/build.gradle.kts, diganti pakai TablerIcon (font glyph) yang
        // sudah ada di com.aumofinance.app.ui.icons.
    }
}
rootProject.name = "AumoFinance"
include(":app")
