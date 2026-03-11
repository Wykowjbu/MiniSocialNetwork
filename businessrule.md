# Business Rules — MiniSocialNetwork

> **Version:** 1.0  
> **Ngày tạo:** 2026-03-11  
> **Tác giả:** Expert Software Architect  
> **Phạm vi áp dụng:** Toàn bộ hệ thống MiniSocialNetwork (ASP.NET Core 8)

---

## Mục lục

1. [Domain: User (Người dùng)](#1-domain-user)
2. [Domain: Post (Bài viết)](#2-domain-post)
3. [Domain: Interaction — Like & Comment](#3-domain-interaction)
4. [Domain: Connection — Follow](#4-domain-connection)
5. [Domain: Security & Validation](#5-domain-security--validation)

---

## Quy ước định dạng

| Cột | Ý nghĩa |
|-----|---------|
| **Rule ID** | Mã định danh duy nhất của rule |
| **Tên Rule** | Mô tả ngắn |
| **Mô tả** | Chi tiết quy tắc |
| **Mức độ ưu tiên** | 🔴 Critical · 🟠 High · 🟡 Medium · 🟢 Low |
| **Hành động xử lý vi phạm** | HTTP Status + Error Message trả về client |

---

## 1. Domain: User

### BR-USR-001 — Username là duy nhất & theo định dạng chuẩn

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-USR-001 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Username phải **duy nhất** trong hệ thống. Chỉ chứa ký tự chữ–số và dấu gạch dưới (`a–z`, `A–Z`, `0–9`, `_`). Độ dài từ **3 đến 30 ký tự**. Không được chứa khoảng trắng hay ký tự đặc biệt. |
| **Hành động xử lý vi phạm** | `400 Bad Request` — `"Username must be 3–30 characters and contain only letters, numbers, and underscores."` / `"Username already exists."` |

---

### BR-USR-002 — Email hợp lệ & duy nhất

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-USR-002 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Email phải đúng định dạng RFC 5322 và **duy nhất** trong hệ thống. Không vượt quá **100 ký tự**. |
| **Hành động xử lý vi phạm** | `400 Bad Request` — `"Invalid email format."` / `"Email already registered."` |

---

### BR-USR-003 — Độ phức tạp mật khẩu

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-USR-003 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Mật khẩu phải: tối thiểu **8 ký tự**, tối đa **128 ký tự**, có ít nhất **1 chữ hoa**, **1 chữ thường**, **1 chữ số**. Không được trùng với username. Phải được hash bằng BCrypt (cost factor ≥ 12) trước khi lưu. |
| **Hành động xử lý vi phạm** | `400 Bad Request` — `"Password must be 8–128 characters and include at least one uppercase letter, one lowercase letter, and one digit."` |

---

### BR-USR-004 — Tài khoản phải ở trạng thái Active

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-USR-004 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Chỉ tài khoản có `IsActive = true` mới được phép đăng nhập và thực hiện bất kỳ thao tác nào trong hệ thống. |
| **Hành động xử lý vi phạm** | `403 Forbidden` — `"Your account has been deactivated. Please contact support."` |

---

### BR-USR-005 — Giới hạn thông tin cá nhân (Profile)

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-USR-005 |
| **Mức độ ưu tiên** | 🟡 Medium |
| **Mô tả** | `FullName` tối đa **100 ký tự**. `Bio` tối đa **300 ký tự**. `DateOfBirth` nếu được cung cấp phải ≥ 13 năm trước ngày hiện tại (yêu cầu độ tuổi tối thiểu). Avatar phải là file ảnh (`jpg`, `jpeg`, `png`, `gif`, `webp`) và không vượt **5 MB**. |
| **Hành động xử lý vi phạm** | `400 Bad Request` — `"Bio cannot exceed 300 characters."` / `"You must be at least 13 years old to register."` / `"Avatar must be an image file (jpg, jpeg, png, gif, webp) under 5MB."` |

---

### BR-USR-006 — Người dùng chỉ được cập nhật thông tin của chính mình

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-USR-006 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Người dùng chỉ được phép chỉnh sửa profile và đổi mật khẩu của tài khoản mình. Không được thay đổi `UserId` hay `Username` sau khi đã đăng ký. |
| **Hành động xử lý vi phạm** | `403 Forbidden` — `"You are not authorized to modify this profile."` |

---

## 2. Domain: Post

### BR-PST-001 — Nội dung bài viết không được rỗng

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-PST-001 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Nội dung (`Content`) của bài viết không được để trống hoặc chỉ chứa khoảng trắng. Độ dài tối thiểu **1 ký tự** (sau khi trim), tối đa **5000 ký tự**. |
| **Hành động xử lý vi phạm** | `400 Bad Request` — `"Post content is required."` / `"Post content cannot exceed 5000 characters."` |

---

### BR-PST-002 — Media đính kèm bài viết

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-PST-002 |
| **Mức độ ưu tiên** | 🟠 High |
| **Mô tả** | Chỉ cho phép đính kèm **1 file ảnh** mỗi bài viết. Định dạng ảnh hợp lệ: `jpg`, `jpeg`, `png`, `gif`. Kích thước tối đa: **10 MB**. Không hỗ trợ upload video. |
| **Hành động xử lý vi phạm** | `400 Bad Request` — `"Only image files (jpg, jpeg, png, gif) are allowed."` / `"Image file size cannot exceed 10MB."` |

---

### BR-PST-003 — Chỉ chủ sở hữu được chỉnh sửa / xóa bài viết

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-PST-003 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Bài viết chỉ được cập nhật hoặc xóa bởi đúng người dùng tạo ra nó (`Post.UserId == currentUserId`). Không có ngoại lệ (trừ Admin role nếu có). |
| **Hành động xử lý vi phạm** | `403 Forbidden` — `"You are not authorized to modify or delete this post."` |

---

### BR-PST-004 — Bài viết phải theo dõi thời điểm chỉnh sửa

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-PST-004 |
| **Mức độ ưu tiên** | 🟡 Medium |
| **Mô tả** | Mỗi khi bài viết được cập nhật, trường `UpdatedAt` phải được ghi nhận thời gian chỉnh sửa. Giao diện nên hiển thị "(đã chỉnh sửa)" nếu `UpdatedAt != null`. |
| **Hành động xử lý vi phạm** | (Logic bắt buộc, không có error message — nếu thiếu `UpdatedAt` là lỗi hệ thống.) |

---

### BR-PST-005 — Giới hạn số bài đăng trong 1 giờ (Anti-spam)

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-PST-005 |
| **Mức độ ưu tiên** | 🟠 High |
| **Mô tả** | Một người dùng không được tạo quá **10 bài viết trong vòng 1 giờ**. Đây là cơ chế chống spam cơ bản. |
| **Hành động xử lý vi phạm** | `429 Too Many Requests` — `"You have reached the posting limit (10 posts/hour). Please wait before posting again."` |

---

## 3. Domain: Interaction

### BR-INT-001 — Không được Like bài viết của chính mình

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-INT-001 |
| **Mức độ ưu tiên** | 🟠 High |
| **Mô tả** | Người dùng **không được** Like bài viết do chính họ tạo ra (`Post.UserId == currentUserId`). |
| **Hành động xử lý vi phạm** | `400 Bad Request` — `"You cannot like your own post."` |

---

### BR-INT-002 — Mỗi người chỉ được Like 1 lần mỗi bài

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-INT-002 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Mỗi người dùng chỉ được thực hiện 1 lượt Like trên mỗi bài viết. Hệ thống hỗ trợ Toggle Like (Like lần 2 = Unlike). |
| **Hành động xử lý vi phạm** | (Xử lý bằng toggle — không trả lỗi, tự động unlike.) |

---

### BR-INT-003 — Comment không được rỗng

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-INT-003 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Nội dung comment không được để trống (sau khi trim). Độ dài tối đa **1000 ký tự**. |
| **Hành động xử lý vi phạm** | `400 Bad Request` — `"Comment content is required."` / `"Comment cannot exceed 1000 characters."` |

---

### BR-INT-004 — Chỉ chủ sở hữu được xóa comment

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-INT-004 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Comment chỉ được xóa bởi người dùng đã tạo ra comment đó (`Comment.UserId == currentUserId`). Chủ bài viết **không** có quyền xóa comment của người khác (trừ Admin). |
| **Hành động xử lý vi phạm** | `403 Forbidden` — `"You are not authorized to delete this comment."` |

---

### BR-INT-005 — Giới hạn số comment trong 1 giờ (Anti-spam)

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-INT-005 |
| **Mức độ ưu tiên** | 🟠 High |
| **Mô tả** | Một người dùng không được tạo quá **30 comment trong vòng 1 giờ**. |
| **Hành động xử lý vi phạm** | `429 Too Many Requests` — `"You have reached the comment limit (30 comments/hour). Please wait before commenting again."` |

---

## 4. Domain: Connection

### BR-CON-001 — Không được Follow chính mình

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-CON-001 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Người dùng **không được** follow chính tài khoản của họ (`followerId == followingId`). |
| **Hành động xử lý vi phạm** | `400 Bad Request` — `"You cannot follow yourself."` |

---

### BR-CON-002 — Không được Follow trùng lặp

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-CON-002 |
| **Mức độ ưu tiên** | 🟠 High |
| **Mô tả** | Mỗi cặp (follower, following) chỉ tồn tại 1 lần trong hệ thống. Follow lần 2 = Unfollow (Toggle). |
| **Hành động xử lý vi phạm** | (Xử lý bằng toggle — không trả lỗi.) |

---

### BR-CON-003 — Không thể tương tác với người dùng không tồn tại

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-CON-003 |
| **Mức độ ưu tiên** | 🟠 High |
| **Mô tả** | Khi thực hiện Follow/Unfollow, hệ thống phải kiểm tra người dùng đích (`followingId`) có tồn tại và `IsActive = true`. |
| **Hành động xử lý vi phạm** | `404 Not Found` — `"User not found."` |

---

### BR-CON-004 — Giới hạn số lượng Following

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-CON-004 |
| **Mức độ ưu tiên** | 🟡 Medium |
| **Mô tả** | Một người dùng không được theo dõi quá **5000 người** (hạn chế tương tự Twitter/X). |
| **Hành động xử lý vi phạm** | `400 Bad Request` — `"You have reached the maximum following limit (5000 users)."` |

---

## 5. Domain: Security & Validation

### BR-SEC-001 — Xác thực danh tính bắt buộc

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-SEC-001 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Tất cả các thao tác tạo/sửa/xóa bài viết, comment, like, follow đều yêu cầu người dùng đã đăng nhập (`currentUserId != null`). Người dùng chưa đăng nhập chỉ được xem nội dung công khai. |
| **Hành động xử lý vi phạm** | `401 Unauthorized` — `"You must be logged in to perform this action."` (Redirect về trang Login.) |

---

### BR-SEC-002 — Quyền sở hữu dữ liệu (Data Ownership)

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-SEC-002 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Mỗi tài nguyên (Post, Comment) có một chủ sở hữu. Chỉ chủ sở hữu (hoặc Admin) mới được phép thực hiện thao tác mutate (UPDATE/DELETE) trên tài nguyên đó. Kiểm tra ownership phải được thực hiện ở **Service Layer**, không chỉ ở View/Controller. |
| **Hành động xử lý vi phạm** | `403 Forbidden` — `"Access denied: you do not own this resource."` |

---

### BR-SEC-003 — Input Sanitization

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-SEC-003 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Tất cả dữ liệu đầu vào từ người dùng phải được **trim** khoảng trắng thừa trước khi xử lý. Không cho phép chứa HTML/script tag trong nội dung Post và Comment (ngăn XSS). ASP.NET Core Razor Pages mặc định encode HTML output — cần **không dùng `@Html.Raw()`** cho user-generated content. |
| **Hành động xử lý vi phạm** | `400 Bad Request` — `"Invalid input detected."` |

---

### BR-SEC-004 — Giới hạn tốc độ đăng ký / đăng nhập (Rate Limiting)

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-SEC-004 |
| **Mức độ ưu tiên** | 🟠 High |
| **Mô tả** | Giới hạn số lần thử đăng nhập thất bại liên tiếp: **5 lần trong 15 phút** từ cùng IP. Giới hạn đăng ký: **3 lần trong 1 giờ** từ cùng IP (chống tạo tài khoản hàng loạt). |
| **Hành động xử lý vi phạm** | `429 Too Many Requests` — `"Too many failed attempts. Please try again in 15 minutes."` |

---

### BR-SEC-005 — Mật khẩu không được lưu dạng plain text

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-SEC-005 |
| **Mức độ ưu tiên** | 🔴 Critical |
| **Mô tả** | Mật khẩu **bắt buộc** phải được hash bằng BCrypt với cost factor ≥ 12 trước khi lưu vào database. Không bao giờ log hay trả về plain text password qua API. |
| **Hành động xử lý vi phạm** | (Lỗi hệ thống nghiêm trọng — phải được bắt ở code review / unit test.) |

---

### BR-SEC-006 — Validate loại và kích thước file upload

| Thuộc tính | Giá trị |
|-----------|---------|
| **Rule ID** | BR-SEC-006 |
| **Mức độ ưu tiên** | 🟠 High |
| **Mô tả** | Khi upload ảnh (avatar hoặc ảnh bài viết), hệ thống phải kiểm tra: (1) Extension file thuộc whitelist (`jpg`, `jpeg`, `png`, `gif`, `webp`), (2) MIME type hợp lệ (không chỉ dựa vào extension), (3) Kích thước ≤ **10 MB** với ảnh bài viết / ≤ **5 MB** với avatar, (4) Đặt tên file ngẫu nhiên (GUID) để tránh path traversal. |
| **Hành động xử lý vi phạm** | `400 Bad Request` — `"Invalid file type or file too large."` |

---

## Tổng hợp Rules theo Mức độ ưu tiên

| Mức độ | Rules |
|--------|-------|
| 🔴 Critical | BR-USR-001, BR-USR-002, BR-USR-003, BR-USR-004, BR-USR-006, BR-PST-001, BR-PST-003, BR-INT-002, BR-INT-003, BR-INT-004, BR-CON-001, BR-SEC-001, BR-SEC-002, BR-SEC-003, BR-SEC-005 |
| 🟠 High | BR-PST-002, BR-PST-005, BR-CON-002 (toggle), BR-CON-003, BR-INT-001, BR-INT-005, BR-SEC-004, BR-SEC-006 |
| 🟡 Medium | BR-USR-005, BR-CON-004, BR-PST-004 |
| 🟢 Low | *(Không có rule ưu tiên thấp trong bản 1.0)* |
