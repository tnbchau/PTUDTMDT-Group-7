function initDashboardChart(canvasId, labels, revenueData, orderData, todayCount) {
    const ctx = document.getElementById(canvasId).getContext('2d');

    const chart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Doanh thu',
                data: revenueData,
                borderColor: '#5D4037',
                backgroundColor: 'rgba(93, 64, 55, 0.1)',
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            onClick: (event, elements) => {
                const titleElem = document.getElementById('order-card-title');
                const valueElem = document.getElementById('order-card-value');

                if (elements.length > 0) {
                    const idx = elements[0].index;
                    titleElem.innerText = `Đơn hàng ngày ${labels[idx]}`;
                    valueElem.innerText = orderData[idx];
                    valueElem.classList.replace('text-success', 'text-primary');
                } else {
                    titleElem.innerText = "Đơn hàng hôm nay";
                    valueElem.innerText = todayCount;
                    valueElem.classList.replace('text-primary', 'text-success');
                }
            }
        }
    });
}