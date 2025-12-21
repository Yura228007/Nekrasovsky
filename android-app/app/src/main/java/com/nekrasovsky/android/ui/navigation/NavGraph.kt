package com.nekrasovsky.android.ui.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import com.nekrasovsky.android.ui.screens.dashboard.DashboardScreen
import com.nekrasovsky.android.ui.screens.login.LoginScreen
import com.nekrasovsky.android.ui.screens.materials.MaterialsScreen
import com.nekrasovsky.android.ui.screens.products.ProductsScreen
import com.nekrasovsky.android.ui.screens.warehouses.WarehousesScreen
import com.nekrasovsky.android.ui.screens.workreports.WorkReportsScreen

sealed class Screen(val route: String) {
    object Login : Screen("login")
    object Dashboard : Screen("dashboard")
    object Products : Screen("products")
    object Materials : Screen("materials")
    object Warehouses : Screen("warehouses")
    object WorkReports : Screen("work_reports")
}

@Composable
fun NavGraph(navController: NavHostController) {
    NavHost(
        navController = navController,
        startDestination = Screen.Login.route
    ) {
        composable(Screen.Login.route) {
            LoginScreen(
                onLoginSuccess = {
                    navController.navigate(Screen.Dashboard.route) {
                        popUpTo(Screen.Login.route) { inclusive = true }
                    }
                }
            )
        }
        
        composable(Screen.Dashboard.route) {
            DashboardScreen(
                onNavigateToProducts = { navController.navigate(Screen.Products.route) },
                onNavigateToMaterials = { navController.navigate(Screen.Materials.route) },
                onNavigateToWarehouses = { navController.navigate(Screen.Warehouses.route) },
                onNavigateToWorkReports = { navController.navigate(Screen.WorkReports.route) },
                onLogout = {
                    navController.navigate(Screen.Login.route) {
                        popUpTo(0) { inclusive = true }
                    }
                }
            )
        }
        
        composable(Screen.Products.route) {
            ProductsScreen(
                onNavigateBack = { navController.popBackStack() }
            )
        }
        
        composable(Screen.Materials.route) {
            MaterialsScreen(
                onNavigateBack = { navController.popBackStack() }
            )
        }
        
        composable(Screen.Warehouses.route) {
            WarehousesScreen(
                onNavigateBack = { navController.popBackStack() }
            )
        }
        
        composable(Screen.WorkReports.route) {
            WorkReportsScreen(
                onNavigateBack = { navController.popBackStack() }
            )
        }
    }
}

