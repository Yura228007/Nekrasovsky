package com.nekrasovsky.android.ui.screens.dashboard

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

data class DashboardItem(
    val title: String,
    val icon: androidx.compose.ui.graphics.vector.ImageVector,
    val onClick: () -> Unit
)

@Composable
fun DashboardScreen(
    onNavigateToProducts: () -> Unit,
    onNavigateToMaterials: () -> Unit,
    onNavigateToWarehouses: () -> Unit,
    onNavigateToWorkReports: () -> Unit,
    onLogout: () -> Unit
) {
    val dashboardItems = listOf(
        DashboardItem(
            title = "Продукты",
            icon = Icons.Default.Inventory,
            onClick = onNavigateToProducts
        ),
        DashboardItem(
            title = "Материалы",
            icon = Icons.Default.Build,
            onClick = onNavigateToMaterials
        ),
        DashboardItem(
            title = "Склады",
            icon = Icons.Default.Storage,
            onClick = onNavigateToWarehouses
        ),
        DashboardItem(
            title = "Отчеты о работе",
            icon = Icons.Default.Assignment,
            onClick = onNavigateToWorkReports
        )
    )

    Scaffold(
        topBar = {
            TopAppBar(
                title = { 
                    Text(
                        "Главная",
                        fontWeight = FontWeight.Bold
                    ) 
                },
                actions = {
                    IconButton(onClick = onLogout) {
                        Icon(
                            imageVector = Icons.Default.Logout,
                            contentDescription = "Выход"
                        )
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer,
                    titleContentColor = MaterialTheme.colorScheme.onPrimaryContainer
                )
            )
        }
    ) { paddingValues ->
        LazyVerticalGrid(
            columns = GridCells.Fixed(2),
            contentPadding = PaddingValues(16.dp),
            horizontalArrangement = Arrangement.spacedBy(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp),
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            items(dashboardItems) { item ->
                DashboardCard(
                    title = item.title,
                    icon = item.icon,
                    onClick = item.onClick
                )
            }
        }
    }
}

@Composable
fun DashboardCard(
    title: String,
    icon: androidx.compose.ui.graphics.vector.ImageVector,
    onClick: () -> Unit
) {
    Card(
        onClick = onClick,
        modifier = Modifier
            .fillMaxWidth()
            .height(150.dp),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.surfaceVariant
        ),
        elevation = CardDefaults.cardElevation(defaultElevation = 4.dp)
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(20.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            Icon(
                imageVector = icon,
                contentDescription = title,
                modifier = Modifier.size(48.dp),
                tint = MaterialTheme.colorScheme.primary
            )
            Spacer(modifier = Modifier.height(12.dp))
            Text(
                text = title,
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.SemiBold,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
    }
}

