// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
const cardItems = localStorage.getItem("cardItems")
    ? JSON.parse(localStorage.getItem("cardItems"))
    : {items:[], totalPrice:0, totalQuantity: 0};

// Write your JavaScript code.
const addTocart = (id, title, price, image, quantity = 1) => {
    if (cardItems?.items.length === 0) {
        cardItems.items.push({ id, title, quantity, image, price });
        cardItems.totalPrice = price;
        cardItems.totalQuantity = 1;
    } else {
        const existingItem = cardItems.items.find((item) => item.id === id);

        if (existingItem) {
            // If the item already exists, update its quantity and price
            existingItem.quantity += quantity;
            existingItem.price += price;

        } else {
            // If the item does not exist, add it to the cardItems array
            cardItems.items.push({ id, title, quantity, image, price });
        }
        cardItems.totalPrice += price;
        cardItems.totalQuantity += quantity;
    }
    
    localStorage.setItem("cardItems", JSON.stringify(cardItems));
};

const showCardItems = () => {
    let orderList = '';
    cardItems.items.forEach((element) => {
    orderList += `
        <tr>
								<td class="px-5 py-5 border-b border-gray-200 bg-white">
									<div class="flex items-center">
										<div class="flex-shrink-0 w-10 h-10">
											<img class="w-10 h-10 rounded-full" 
                                                style="height:50px; width: 50px;"
                                                src=${element.image}
                                                alt="" />
                                        </div>
											<div class="ml-3">
												<p class="text-gray-900 whitespace-no-wrap">
													${element.title}
												</p>
											</div>
										</div>
								</td>
								<td class="px-5 py-5 border-b border-gray-200 bg-white ">
									<p class="text-gray-900 whitespace-no-wrap">&dollar; ${Math.floor(element.price)}</p>
								</td>
								<td class="px-5 py-5 border-b border-gray-200 bg-white ">
									<p class="text-gray-900 whitespace-no-wrap">
										${element.quantity}
									</p>
								</td>
								
								<td class="px-5 py-5 border-b border-gray-200 bg-white ">
									<span
                                        class="relative inline-block px-3 py-1 font-semibold text-green-900 leading-tight">
                                        <span aria-hidden
                                            class="absolute inset-0 bg-green-200 opacity-50 rounded-full"></span>
									<span class="relative">remove</span>
									</span>
								</td>
							</tr>
    `
    })
    orderList += `
    <tr>
                            <td class="px-5 py-5 border-b border-gray-200 bg-white">
                                        Total
                            </td>
                            <td class="px-5 py-5 border-b border-gray-200 bg-white ">
                                <p class="text-gray-900 whitespace-no-wrap">Total price:<br/> &dollar; ${Math.floor(cardItems.totalPrice)}</p>
                            </td>
                            <td class="px-5 py-5 border-b border-gray-200 bg-white ">
                                <p class="text-gray-900 whitespace-no-wrap">
                                    total Quantity: ${cardItems.totalQuantity}
                                </p>
                            </td>
                        </tr>
`    
    document.getElementById("order-body").innerHTML = orderList;
};

showCardItems();
