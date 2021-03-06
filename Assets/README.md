## Алгоритм решения задачи
1. Пробегаюсь по всем вертексам мешей объектов (хотя эта точность избыточная сейчас из-за недостатков решения)
2. Перевожу все вертексы в сферические координаты
3. В сферических координатах нахожу среди всех точек максимальные и минимальные polar и elevation координаты
4. Из этих четырех точек вычисляю вектор в сферических координатах, который дает направление видимого центра. Перевожу в декартовы кординаты и поворачиваю камеру по вектору.
5. Из разницы между максимальными и минимальными координатами получаю требуемый для отображения FoV в соответствующих плоскостях. Перевожу горизонтальный FoV в вертикальный, сравниваю и присваиваю камере наибольший из двух.

Решение получается неточным. Крайние видимые точки объектов не всегда касаются плоскостей фрустума камеры. А также горизонтальные границы некорректно отображают реальные ограничения положения объектов по горизонтальной оси камеры.
Первая и самая значимая проблема заключается, верятно, в том, что сферические координаты не позволяют учесть перспективные искажения. И, возможно, следует использовать однородные координаты.
